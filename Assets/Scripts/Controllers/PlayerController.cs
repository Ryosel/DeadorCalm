using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : LivingEntity
{
    [SerializeField]
    float _moveSpeed = 5f; // �յ� �������� �ӵ�
    [SerializeField]
    float _rotateSpeed = 180f; // �¿� ȸ�� �ӵ�

    Camera _followCam;

    float _speed = 5f;
    float _speedSmoothTime = 0.1f;
    float _turnSmoothTime = 0.1f;
    float _speedSmoothVelocity;
    float _turnSmoothVelocity;
    float _currentVelocityY;
    float _currentSpeed => _playerRigidbody.velocity.magnitude;

    [SerializeField]
    Gun _gun; // ����� ��
    [SerializeField]
    Transform _gunPivot; // �� ��ġ�� ������
    [SerializeField]
    Transform _leftHandMount; // ���� ���� ������, �޼��� ��ġ�� ����
    [SerializeField]
    Transform _rightHandMount; // ���� ������ ������, �������� ��ġ�� ����

    [SerializeField]
    UI_HealthSlider _healthSlider;

    Rigidbody _playerRigidbody; // �÷��̾� ĳ������ ������ٵ�
    Animator _playerAnimator; // �÷��̾� ĳ������ �ִϸ�����

    public Gun Gun { get { return _gun; } }

    public Vector2 MoveInput { get; private set; }
    public float Move { get; private set; } // ������ ������ �Է°�
    public float Rotate { get; private set; } // ������ ȸ�� �Է°�
    public bool Fire { get; private set; } // ������ �߻� �Է°�
    public bool Reload { get; private set; } // ������ ������ �Է°�

    void Start()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerAnimator = GetComponent<Animator>();
        _followCam = Camera.main;

        OnDeath -= Managers.Game.EndGame;
        OnDeath += Managers.Game.EndGame;

        Managers.Input.KeyAction -= OnKey;
        Managers.Input.KeyAction += OnKey;
        Managers.Input.KeyAction -= OnTab;
        Managers.Input.KeyAction += OnTab;
    }

    void OnTab()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Managers.Game.Mode == Define.GameMode.Infil)
                Managers.Game.ChangeGameMode(Define.GameMode.Recon);
            else if (Managers.Game.Mode == Define.GameMode.Recon)
                Managers.Game.ChangeGameMode(Define.GameMode.Infil);
        }
    }

    void OnKey()
    {
        // ���ӿ��� ���¿����� ����� �Է��� �������� �ʴ´�
        if (Managers.Game.IsGameover || Managers.Game.Mode != Define.GameMode.Infil)
        {
            MoveInput = Vector2.zero;
            Move = 0;
            Rotate = 0;
            Fire = false;
            Reload = false;
            return;
        }

        MoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (MoveInput.sqrMagnitude > 1)
            MoveInput = MoveInput.normalized;

        // move�� ���� �Է� ����
        Move = Input.GetAxis("Vertical");
        // rotate�� ���� �Է� ����
        Rotate = Input.GetAxis("Horizontal");
        // fire�� ���� �Է� ����
        Fire = Input.GetButton("Fire1");
        // reload�� ���� �Է� ����
        Reload = Input.GetButtonDown("Reload");
    }

    void FixedUpdate()
    {

        if (MoveInput.magnitude > 0.9f || Fire)
            RotateUpdate();
        MoveUpdate();
    }

    void Update()
    {
        if (Dead)
            return;

        // �ֱ⸶�� ������, ȸ��, �ִϸ��̼� ó�� ����
        //RotateUpdate();
        //MoveUpdate();

        _playerAnimator.SetFloat("Move", MoveInput.magnitude);

        // �Է��� �����ϰ� �� �߻��ϰų� ������
        if (Fire)
            _gun.Fire();
        else if (Reload)
        {
            if (_gun.Reload())
            {
                _playerAnimator.SetTrigger("Reload");
            }
        }

        UpdateUI();
    }

    void MoveUpdate()
    {
        //Vector3 moveDistance = Move * transform.forward * _moveSpeed * Time.deltaTime;

        // _playerRigidbody.MovePosition(_playerRigidbody.position + moveDistance);
        var targetSpeed = _speed * MoveInput.magnitude;
        var moveDirection = Vector3.Normalize(transform.forward * MoveInput.y + transform.right * MoveInput.x);

        var smootTime = _speedSmoothTime;

        //targetSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, smootTime);
        //Debug.Log($"moveinput mag {MoveInput.magnitude} curspeed {_playerRigidbody.velocity.magnitude} targetspeed {_targetSpeed}");
        var velocity = moveDirection * targetSpeed;

        _playerRigidbody.MovePosition(_playerRigidbody.position + velocity * Time.deltaTime);
    }

    void RotateUpdate()
    {
        //float turn = Rotate * _rotateSpeed * Time.deltaTime;

        //_playerRigidbody.rotation = _playerRigidbody.rotation * Quaternion.Euler(0, turn, 0f);
        var targetRotation = _followCam.transform.eulerAngles.y;

        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,
            ref _turnSmoothVelocity, _turnSmoothTime);
    }

    // ź�� UI ����
    void UpdateUI()
    {
        if (_gun != null)
        {
            // UI �Ŵ����� ź�� �ؽ�Ʈ�� źâ�� ź��� ���� ��ü ź���� ǥ��
            UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
            ui.UpdateAmmoText(_gun.MagAmmo, _gun.AmmoRemain);
        }
    }

    // �ִϸ������� IK ����
    void OnAnimatorIK(int layerIndex)
    {
        _gunPivot.position = _playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow);

        _playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        _playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        _playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandMount.position);
        _playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandMount.rotation);

        _playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        _playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        _playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandMount.position);
        _playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandMount.rotation);
    }

    // ü�� ȸ��
    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth);
    }

    // ������ ó��
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!Dead)
        {
            Managers.Sound.Play("Woman Damage");
        }

        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // ��� ó��
    public override void Die()
    {
        base.Die();

        _healthSlider.gameObject.SetActive(false);

        Managers.Sound.Play("Woman Die");
        _playerAnimator.SetTrigger("Die");

        _gun.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Dead)
        {
            IItem item = other.GetComponent<IItem>();

            if (item != null)
            {
                item.Use(gameObject);
                Managers.Sound.Play("Pick Up");
            }
        }
    }
}
