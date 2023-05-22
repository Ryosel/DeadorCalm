using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : LivingEntity
{
    [SerializeField]
    float _moveSpeed = 5f; // 앞뒤 움직임의 속도
    [SerializeField]
    float _rotateSpeed = 180f; // 좌우 회전 속도

    Camera _followCam;

    float _speed = 5f;
    float _speedSmoothTime = 0.1f;
    float _turnSmoothTime = 0.1f;
    float _speedSmoothVelocity;
    float _turnSmoothVelocity;
    float _currentVelocityY;
    float _currentSpeed => _playerRigidbody.velocity.magnitude;

    [SerializeField]
    Gun _gun; // 사용할 총
    [SerializeField]
    Transform _gunPivot; // 총 배치의 기준점
    [SerializeField]
    Transform _leftHandMount; // 총의 왼쪽 손잡이, 왼손이 위치할 지점
    [SerializeField]
    Transform _rightHandMount; // 총의 오른쪽 손잡이, 오른손이 위치할 지점

    [SerializeField]
    UI_HealthSlider _healthSlider;

    Rigidbody _playerRigidbody; // 플레이어 캐릭터의 리지드바디
    Animator _playerAnimator; // 플레이어 캐릭터의 애니메이터

    public Gun Gun { get { return _gun; } }

    public Vector2 MoveInput { get; private set; }
    public float Move { get; private set; } // 감지된 움직임 입력값
    public float Rotate { get; private set; } // 감지된 회전 입력값
    public bool Fire { get; private set; } // 감지된 발사 입력값
    public bool Reload { get; private set; } // 감지된 재장전 입력값

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
        // 게임오버 상태에서는 사용자 입력을 감지하지 않는다
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

        // move에 관한 입력 감지
        Move = Input.GetAxis("Vertical");
        // rotate에 관한 입력 감지
        Rotate = Input.GetAxis("Horizontal");
        // fire에 관한 입력 감지
        Fire = Input.GetButton("Fire1");
        // reload에 관한 입력 감지
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

        // 주기마다 움직임, 회전, 애니메이션 처리 실행
        //RotateUpdate();
        //MoveUpdate();

        _playerAnimator.SetFloat("Move", MoveInput.magnitude);

        // 입력을 감지하고 총 발사하거나 재장전
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

    // 탄약 UI 갱신
    void UpdateUI()
    {
        if (_gun != null)
        {
            // UI 매니저의 탄약 텍스트에 탄창의 탄약과 남은 전체 탄약을 표시
            UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
            ui.UpdateAmmoText(_gun.MagAmmo, _gun.AmmoRemain);
        }
    }

    // 애니메이터의 IK 갱신
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

    // 체력 회복
    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth);
    }

    // 데미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!Dead)
        {
            Managers.Sound.Play("Woman Damage");
        }

        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
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
