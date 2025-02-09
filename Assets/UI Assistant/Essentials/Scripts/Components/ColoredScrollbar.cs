using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIAssistant
{
    [HelpURL("https://sites.google.com/view/uiassistant/documentation/color?authuser=0#h.opnv8e9evao9")]
    [AddComponentMenu("UI Assistant/Colored Scrollbar"), RequireComponent(typeof(GraphicColorizerGroup)), DisallowMultipleComponent]
    public class ColoredScrollbar : Scrollbar
    {
        #region Variables
        [SerializeField, HideInInspector] GraphicColorizerGroup GraphicColorizerGroup;
        #endregion

        #region Function
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            UIAssistant.SelectionState convertedState;
            if (state == SelectionState.Disabled && interactable)
                convertedState = UIAssistant.SelectionState.Normal;
            else convertedState = (UIAssistant.SelectionState)state;

            GraphicColorizerGroup.DoStateTransition(convertedState, instant);
        }
        #endregion

#if UNITY_EDITOR

        #region Function
        protected override void OnValidate()
        {
            base.OnValidate();

            if (GraphicColorizerGroup == null) GraphicColorizerGroup = GetComponent<GraphicColorizerGroup>();
        }
        #endregion

#endif
    }
}