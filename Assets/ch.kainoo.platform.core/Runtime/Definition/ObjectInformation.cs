using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.platform
{
    [CreateAssetMenu(
        fileName = "object_information", 
        menuName = "Kainoo/Platform/Create Object Information", 
        order = 1
        )]
    public class ObjectInformation : ScriptableObject
    {
        public GameObject MainObject;
    }

}