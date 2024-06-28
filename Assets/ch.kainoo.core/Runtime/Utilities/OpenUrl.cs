using UnityEngine;

namespace ch.kainoo.core
{

    public class OpenUrl : MonoBehaviour
    {
        public void Open(string url)
        {
            Application.OpenURL(url);
        }
    }

}