﻿using Events;
using Scripts.EventBus;
using UI;
using UI.Elements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities.Extensions;

namespace States.GameplayUIStates
{
    public class PlacingShips : BaseState
    {
        private VisualElement _container;

        private Label _countDownLabel;
        private EventBinding<OnCountdownUpdated> _onCountdownUpdatedBinding;
        private Toggle _readyToggle;
        private StyledButton _styledButton;

        public PlacingShips(GameplayUIManager gameplayUIManager, StyleSheet styleSheet) : base(gameplayUIManager)
        {
            var uiDocument = new GameObject(nameof(PlacingShips)).AddComponent<UIDocument>();
            uiDocument.panelSettings = GameResources.Instance.UIDocumentPrefab.panelSettings;
            uiDocument.visualTreeAsset = GameResources.Instance.UIDocumentPrefab.visualTreeAsset;
            Root = uiDocument.rootVisualElement;

            Root.styleSheets.Add(styleSheet);
        }

        protected sealed override VisualElement Root { get; }


        protected override void GenerateView()
        {
            _container = Root.CreateChild("container");
            VisualElement center = _container.CreateChild("countdown-container", "flex-center");
            VisualElement buttons = _container.CreateChild("buttons-container", "flex-center");

            _styledButton = new StyledButton(GameplayUIManager.ThemeSettings, buttons, "randomize-btn");
            _readyToggle = new StyledToggle(GameplayUIManager.ThemeSettings, buttons, "ready-toggle");
            _countDownLabel = new Label("1") { visible = false };

            center.CreateChild("countdown-text", "flex-center").Add(_countDownLabel);
        }


        protected override void SetVisible(bool value) => Root.visible = value;

        public override void OnEnter()
        {
            base.OnEnter();

            _onCountdownUpdatedBinding = new EventBinding<OnCountdownUpdated>(Countdown_OnSecondPassed);
            EventBus<OnCountdownUpdated>.Register(_onCountdownUpdatedBinding);

            _readyToggle.RegisterCallback<MouseUpEvent>(_ => ReadyToggle_OnValueChanged(_readyToggle.value));

            _styledButton.clicked += () => EventBus<OnRandomizeButtonClicked>.Invoke(new OnRandomizeButtonClicked());
        }

        public override void OnExit()
        {
            base.OnExit();

            EventBus<OnCountdownUpdated>.Deregister(_onCountdownUpdatedBinding);
            _readyToggle.UnregisterCallback<MouseUpEvent>(_ => ReadyToggle_OnValueChanged(_readyToggle.value));
        }

        private void Countdown_OnSecondPassed(OnCountdownUpdated e)
        {
            Vector3 targetPosition = _container.transform.position + Vector3.left * 10;
            Vector3 startPosition = _container.transform.position + Vector3.right * 10;

            _countDownLabel.text = e.Seconds.ToString();
            _countDownLabel.experimental.animation.Position(targetPosition, 1000).from = startPosition;
            _countDownLabel.experimental.animation
                    .Start(0, 1, 500, (element, value) =>
                        element.style.opacity = new StyleFloat(value))
                    .onAnimationCompleted +=
                () => _countDownLabel.experimental.animation
                    .Start(1, 0, 500, (element, value) =>
                        element.style.opacity = new StyleFloat(value));
        }

        private void ReadyToggle_OnValueChanged(bool isOn)
        {
            _countDownLabel.visible = isOn;
            EventBus<OnReadyUIButtonToggled>.Invoke(new OnReadyUIButtonToggled(_readyToggle));
        }
    }
}