using Domains.View.Widgets;
using Game.Scenes.Adventure;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    // Role:
    // Receives all widget-originated commands and forwards accepted user intent to UI flows or the screen controller.
    public sealed partial class AdventureView
    {
        internal async void OnWidgetPouchClicked()
        {
            bool commandStarted = false;

            try
            {
                int screenLifetimeVersion = _screenLifetimeVersion;
                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                if (!_controller.CanClickPouch())
                    return;

                if (!TryBeginWidgetCommand())
                    return;

                commandStarted = true;

                await _screenUIFlow.PlayPouchClicked(
                    _screenWidgets.Pouch,
                    _screenWidgets.CoinStatus,
                    () => IsCurrentScreenLifetime(screenLifetimeVersion));

                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                _controller.OnPouchClicked();
            }
            catch (System.Exception exception)
            {
                LogAsyncException(exception);
            }
            finally
            {
                if (commandStarted)
                    EndWidgetCommand();
            }
        }

        internal async void OnWidgetEndTurnClicked()
        {
            bool commandStarted = false;

            try
            {
                int screenLifetimeVersion = _screenLifetimeVersion;
                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                if (!_controller.CanClickEndTurn())
                    return;

                if (!TryBeginWidgetCommand())
                    return;

                commandStarted = true;

                await _screenUIFlow.PlayEndTurnClicked(
                    _screenWidgets.CoinStatus,
                    _screenWidgets.EndTurn,
                    () => IsCurrentScreenLifetime(screenLifetimeVersion));

                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                await _controller.OnEndTurnClicked();
            }
            catch (System.Exception exception)
            {
                LogAsyncException(exception);
            }
            finally
            {
                if (commandStarted)
                    EndWidgetCommand();
            }
        }

        internal void OnWidgetCardPointerEntered(VisualElement card, uint cardId)
        {
            if (_skillUIFlow.IsTargetingActive &&
                !_controller.CanUseSkillOnTarget(_skillUIFlow.ActiveSkillHandle, cardId))
            {
                return;
            }

            _skillUIFlow.HoverCard(card);
        }

        internal void OnWidgetCardPointerLeft(VisualElement card, uint cardId)
        {
            _skillUIFlow.LeaveCard(card);
        }

        internal async void OnWidgetCardClicked(VisualElement card, uint cardId)
        {
            bool commandStarted = false;

            try
            {
                int screenLifetimeVersion = _screenLifetimeVersion;
                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                bool isTargeting = _skillUIFlow.IsTargetingActive;
                if (!isTargeting && !_controller.CanClickCard(cardId))
                    return;

                if (isTargeting &&
                    !_controller.CanUseSkillOnTarget(_skillUIFlow.ActiveSkillHandle, cardId))
                {
                    return;
                }

                if (!TryBeginWidgetCommand())
                    return;

                commandStarted = true;

                if (!isTargeting)
                {
                    await _controller.OnCardClicked(cardId);
                    return;
                }

                if (!_controller.CanUseSkillOnTarget(_skillUIFlow.ActiveSkillHandle, cardId))
                    return;

                _skillUIFlow.HoverCard(card);
                await ConfirmSkillTarget();
            }
            catch (System.Exception exception)
            {
                LogAsyncException(exception);
            }
            finally
            {
                if (commandStarted)
                    EndWidgetCommand();
            }
        }

        internal async void OnWidgetSkillSlotSelectionChanged(int selectedIndex, SkillSlotWidget selectedButton)
        {
            bool commandStarted = false;

            try
            {
                int screenLifetimeVersion = _screenLifetimeVersion;
                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                if (_widgetCommandRunning)
                {
                    _skillUIFlow.Clear();
                    return;
                }

                AdventureSkillSelectionResult result =
                    _skillUIFlow.SelectSkill(selectedIndex, selectedButton);

                if (!result.HasSkill)
                    return;

                if (!_controller.CanUseSkill(result.Handle))
                {
                    _skillUIFlow.Clear();
                    return;
                }

                if (!result.ShouldActivateImmediately)
                    return;

                if (!TryBeginWidgetCommand())
                    return;

                commandStarted = true;

                await _controller.UseSkill(result.Handle);
                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                _skillUIFlow.Clear();
            }
            catch (System.Exception exception)
            {
                LogAsyncException(exception);
            }
            finally
            {
                if (commandStarted)
                    EndWidgetCommand();
            }
        }
    }
}
