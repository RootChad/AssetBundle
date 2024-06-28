using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core.utilities {

    public class SharedGameObject : MonoBehaviour {

        [SerializeField]
        private bool disableOnStart = true;

        private HashSet<MonoBehaviour> requesters = new HashSet<MonoBehaviour>();

        private void Awake() {
            if (disableOnStart) {
                gameObject.SetActive(false);
            }
        }

        public void Enable(MonoBehaviour mb) {
            requesters.Add(mb);
            gameObject.SetActive(true);

        }

        public void Disable(MonoBehaviour mb) {
            if (requesters.Remove(mb)) {
                if (requesters.Count == 0) {
                    gameObject.SetActive(false);
                }
            } else {
                Debug.LogWarning($"SharedGameObject.Disable: requester ('{mb.name}') was not registered");
            }

        }

    }

}