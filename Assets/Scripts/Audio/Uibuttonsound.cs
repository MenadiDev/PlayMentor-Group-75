using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
            AudioManager.Instance?.PlayButtonClick());
    }
}
