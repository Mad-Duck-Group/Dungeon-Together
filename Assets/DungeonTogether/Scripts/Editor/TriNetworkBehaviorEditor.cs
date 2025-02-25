using UnityEngine;

#region
using TriInspector.Editors;

using Unity.Netcode;

using UnityEditor;
#endregion

[CustomEditor(typeof(NetworkBehaviour), true)]
public class TriNetworkBehaviorEditor : TriEditor { }
