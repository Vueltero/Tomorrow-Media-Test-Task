using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TomorrowMedia.TestTask.Pokemon
{
    public class PokemonInfoController : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        public void Init(PokemonData pokemonData)
        {
            _iconImage.sprite = pokemonData.sprite;
            _titleText.text = pokemonData.name;
            _descriptionText.text = pokemonData.description;
        }
    }
}
