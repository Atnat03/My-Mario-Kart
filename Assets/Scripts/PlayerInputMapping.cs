using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMapping : MonoBehaviour
{
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] KartController _kartController;
    [SerializeField] PlayerItem _playerItem;

    private InputAction _moveAction;
    private InputAction _driftAction;
    private InputAction _dropItemAction;
    private InputAction _ifFrontAction;

    private void OnEnable()
    {
        if (_playerInput == null || _playerInput.actions == null) return;

        _moveAction     = _playerInput.actions["Move"];
        _driftAction    = _playerInput.actions["Drift"];
        _dropItemAction = _playerInput.actions["DropItem"];
        _ifFrontAction  = _playerInput.actions["IsFront"];

        _moveAction.performed     += HandleMove;
        _moveAction.canceled      += HandleMove;
        _driftAction.performed    += HandleDrift;
        _driftAction.canceled     += HandleDrift;
        _dropItemAction.performed += HandleDrop;
        _dropItemAction.canceled  += HandleDrop;
        _ifFrontAction.performed  += HandleFront;
        _ifFrontAction.canceled   += HandleFront;
    }

    private void OnDisable()
    {
        if (_moveAction != null)
        {
            _moveAction.performed -= HandleMove;
            _moveAction.canceled  -= HandleMove;
        }
        if (_driftAction != null)
        {
            _driftAction.performed -= HandleDrift;
            _driftAction.canceled  -= HandleDrift;
        }
        if (_dropItemAction != null)
        {
            _dropItemAction.performed -= HandleDrop;
            _dropItemAction.canceled  -= HandleDrop;
        }
        if (_ifFrontAction != null)
        {
            _ifFrontAction.performed -= HandleFront;
            _ifFrontAction.canceled  -= HandleFront;
        }
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        _kartController.HorizontalInput = input.x;
        _kartController.VerticalInput = input.y;
    }

    private void HandleDrift(InputAction.CallbackContext ctx)
    {
        _kartController.IsDrifing = ctx.ReadValueAsButton();
    }
    
    
    private void HandleFront(InputAction.CallbackContext ctx)
    {
        _playerItem.IsFront = ctx.ReadValueAsButton();
    }

    private void HandleDrop(InputAction.CallbackContext ctx)
    {
        _playerItem.DropItem();
    }
}
