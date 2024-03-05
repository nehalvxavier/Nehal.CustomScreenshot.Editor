using System.Collections.Generic;
using UnityEngine;

namespace NehalVXavier.Tools
{
    [CreateAssetMenu(fileName = "ResolutionSettings", menuName = "Custom/Resolution Settings")]
    public class ResolutionSettings : ScriptableObject
    {
        public List<string> resolutionNames = new List<string>();
        public List<string> filePaths = new List<string>();
        public List<Vector2Int> resolutions = new List<Vector2Int>();
        public List<int> numberOfScreenshots = new List<int>();
    }
}

