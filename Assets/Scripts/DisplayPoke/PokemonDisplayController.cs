using System.Collections;
using System.Collections.Generic;
using TomorrowMedia.TestTask.Core;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace TomorrowMedia.TestTask.Pokemon
{
    public class PokemonDisplayController : MonoBehaviour
    {
        [SerializeField] private PokemonInfoController _pokemonInfoController;
        [SerializeField] private RectTransform _pokemonsContainerRect;

        private List<PokemonData> _pokemonList = new();
        private int _offset;

        private void Awake()
        {
            StartCoroutine(GetPokemonsFromAPI(Constants.Values.POKEMON_LIMIT, _offset));
        }

        public void ArrowButtonClick(bool isNext)
        {
            _offset = System.Math.Max(0, _offset + (isNext ? Constants.Values.POKEMON_LIMIT : -Constants.Values.POKEMON_LIMIT));

            StartCoroutine(GetPokemonsFromAPI(Constants.Values.POKEMON_LIMIT, _offset));
        }

        /// <summary>
        /// Get all pokemons from the API. Saves them on a list, and then displays them
        /// </summary>
        /// <param name="limit">How many pokemons to get</param>
        /// <param name="offset">From what position it starts getting them</param>
        public IEnumerator GetPokemonsFromAPI(int limit, int offset)
        {
            _pokemonList.Clear(); //reset list

            UnityWebRequest request = UnityWebRequest.Get($"{Constants.ApiUrls.POKEMON}?limit={limit}&offset={offset}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading pokemon API: " + request.error);
                yield break;
            }

            JObject json = JObject.Parse(request.downloadHandler.text);
            JArray results = (JArray)json["results"];

            foreach (JToken item in results)
            {
                string name = item["name"].ToString();
                string pokemonUrl = item["url"].ToString();
                yield return StartCoroutine(GetPokemonData(name, pokemonUrl));
            }

            DisplayPokemons(_pokemonList);
        }

        /// <summary>
        /// Get a pokemon imageUrl and description, then downloads the image
        /// </summary>
        /// <param name="name">Name of the pokemon, to search on the pokemon species</param>
        /// <param name="url">Url of the pokemon</param>
        /// <returns></returns>
        private IEnumerator GetPokemonData(string name, string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading pokemon data: " + request.error);
                yield break;
            }

            JObject json = JObject.Parse(request.downloadHandler.text);
            string imageUrl = json["sprites"]["front_default"].ToString();

            string speciesUrl = $"{Constants.ApiUrls.POKEMON_SPECIES}/{name}/"; //to get it's description
            UnityWebRequest speciesRequest = UnityWebRequest.Get(speciesUrl);

            yield return speciesRequest.SendWebRequest();

            string description = "-"; //default text
            if (speciesRequest.result == UnityWebRequest.Result.Success)
            {
                JObject speciesJson = JObject.Parse(speciesRequest.downloadHandler.text);
                JArray flavorTexts = (JArray)speciesJson["flavor_text_entries"];

                foreach (JToken entry in flavorTexts)
                {
                    if (entry["language"]["name"].ToString() == "en") //english description
                    {
                        description = entry["flavor_text"].ToString().Replace("\n", " ").Replace("\f", " ");
                        break;
                    }
                }
            }

            yield return StartCoroutine(DownloadImage(imageUrl, name, description));
        }

        /// <summary>
        /// Download the pokemon's image, converts it to a sprite and add the new pokemon to the list
        /// </summary>
        private IEnumerator DownloadImage(string imageUrl, string name, string description)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading pokemon image: " + request.error);
                yield break;
            }

            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            //all data had been saved, add new pokemon to list
            _pokemonList.Add(new PokemonData(sprite, name, description));
        }

        private void DisplayPokemons(List<PokemonData> pokemons)
        {
            //reset display
            foreach (Transform child in _pokemonsContainerRect)
                if (child.TryGetComponent(out PokemonInfoController controller))
                    Destroy(child.gameObject);

            //display pokemons
            _pokemonList.ForEach(pokemonData => Instantiate(_pokemonInfoController, _pokemonsContainerRect).Init(pokemonData));
        }
    }
}
