using System;
using DungeonTogether.Scripts.Utils;
using UnityEditor.Timeline.Actions;

namespace DungeonTogether.Scripts.Character
{
    public enum CharacterMovementState
    {
        Idle,
        Walking,
        Dashing
    }

    public struct MovementStateEvent
    {
        public CharacterHub characterHub;
        public CharacterMovementState previousState;
        public CharacterMovementState newState;

        private static MovementStateEvent _eventData;

        public static void Invoke(CharacterHub characterHub, CharacterMovementState previousState,
            CharacterMovementState newState)
        {
            _eventData.characterHub = characterHub;
            _eventData.previousState = previousState;
            _eventData.newState = newState;
            EventBus<MovementStateEvent>.Invoke(_eventData);
        }
    }

    public enum CharacterActionState
    {
        None,
        Basic,
        Skill,
        Ultimate
    }

    public struct ActionStateEvent
    {
        public CharacterHub characterHub;
        public CharacterActionState previousState;
        public CharacterActionState newState;

        private static ActionStateEvent _eventData;

        public static void Invoke(CharacterHub characterHub, CharacterActionState previousState,
            CharacterActionState newState)
        {
            _eventData.characterHub = characterHub;
            _eventData.previousState = previousState;
            _eventData.newState = newState;
            EventBus<ActionStateEvent>.Invoke(_eventData);
        }
    }

    public enum CharacterConditionState
    {
        Normal,
        Stunned,
        Dead
    }

    public struct ConditionStateEvent
    {
        public CharacterHub characterHub;
        public CharacterConditionState previousState;
        public CharacterConditionState newState;

        private static ConditionStateEvent _eventData;

        public static void Invoke(CharacterHub characterHub, CharacterConditionState previousState,
            CharacterConditionState newState)
        {
            _eventData.characterHub = characterHub;
            _eventData.previousState = previousState;
            _eventData.newState = newState;
            EventBus<ConditionStateEvent>.Invoke(_eventData);
        }
    }
}