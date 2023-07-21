using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Invertex.UnityEditorTools
{
    public static class ObjectChangeTracker
    {
        /// <summary>
        /// Triggered when a new GameObject is created in Edit mode, not simply a new instance of an object.
        /// </summary>
        public static event System.Action<GameObject> OnGameObjectCreated;

        private static System.Func<int, UnityEngine.Object> _findObjFromInstID;

        [InitializeOnLoadMethod()]
        private static void Initialize()
        {
            var methodInfo = typeof(UnityEngine.Object).GetMethod("FindObjectFromInstanceID", BindingFlags.NonPublic | BindingFlags.Static);

            if (methodInfo == null) { Debug.LogError("Couldn't find 'FindObjectFromInstanceID' in the UnityEngine.Object class. ObjectChangeTracker disabled."); return; }
             _findObjFromInstID = (System.Func<int, UnityEngine.Object>)System.Delegate.CreateDelegate(typeof(System.Func<int, UnityEngine.Object>), methodInfo);

            ObjectChangeEvents.changesPublished -= ChangesPublished;
            ObjectChangeEvents.changesPublished += ChangesPublished;
        }

     
        static void ChangesPublished(ref ObjectChangeEventStream objEvents)
        {
            for(int i = 0; i < objEvents.length; i ++)
            {
                var eventType = objEvents.GetEventType(i);
                if(eventType == ObjectChangeKind.CreateGameObjectHierarchy)
                {
                    objEvents.GetCreateGameObjectHierarchyEvent(i, out var objCreateEvt);
                    GameObjectCreated(objCreateEvt);
                }
            }
        }

        private static void GameObjectCreated(CreateGameObjectHierarchyEventArgs objCreateEvt)
        {
            var obj = _findObjFromInstID(objCreateEvt.instanceId) as GameObject;
            if (obj != null) { OnGameObjectCreated?.Invoke(obj); }
        }
    }
}
