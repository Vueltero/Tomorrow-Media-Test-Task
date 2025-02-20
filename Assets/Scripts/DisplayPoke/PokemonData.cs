using UnityEngine;

namespace TomorrowMedia.TestTask.Pokemon
{
    [System.Serializable]
    public class PokemonData
    {
        public Sprite sprite;
        public string name;
        public string description;

        public PokemonData(Sprite _image, string _title, string _description)
        {
            sprite = _image;
            name = _title;
            description = _description;
        }
    }
}
