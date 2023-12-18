using UnityEngine;

public class UpdatableData : ScriptableObject
{

    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (autoUpdate)
                UnityEditor.EditorApplication.delayCall += NotifyOfUpdatedValues;
        }

        public void NotifyOfUpdatedValues()
        {
            UnityEditor.EditorApplication.delayCall -= NotifyOfUpdatedValues;
            if (OnValuesUpdated != null)
                OnValuesUpdated();
        }
    #endif
}