using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : LivingEntity
{
    [SerializeField]
    LayerMask _whatIsTarget; // 추적 대상 레이어

    LivingEntity _targetEntity; // 추적 대상
    NavMeshAgent _navMeshAgent; // 경로 계산 AI 에이전트

    [SerializeField]
    ParticleSystem _hitEffect; // 피격 시 재생할 파티클 효과

    Animator _zombieAnimator; // 애니메이터 컴포넌트
    Renderer _zombieRenderer; // 렌더러 컴포넌트

    [SerializeField]
    float _damage = 20f; // 공격력
    [SerializeField]
    float _timeBetAttack = 0.5f; // 공격 간격
    float _lastAttackTime; // 마지막 공격 시점

    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    bool HasTarget
    {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (_targetEntity != null && !_targetEntity.Dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake()
    {
        // 초기화
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _zombieAnimator = GetComponent<Animator>();
        _zombieRenderer = GetComponentInChildren<Renderer>();
    }

    // 좀비 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(ZombieData zombieData)
    {
        StartingHealth = zombieData.health;
        Health = zombieData.health;
        _damage = zombieData.damage;
        _navMeshAgent.speed = zombieData.speed;
        _zombieRenderer.material.color = zombieData.skinColor;
    }

    void Start()
    {

        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션 재생
        _zombieAnimator.SetBool("HasTarget", HasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로 갱신
    IEnumerator UpdatePath()
    {
        // 살아 있는 동안 무한 루프
        while (!Dead)
        {
            if (HasTarget)
            {
                // 추적 대상 존재 : 경로를 갱신하고 AI 이동을 계속 진행
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(_targetEntity.transform.position);
            }
            else
            {
                // 추적 대상 없음 : AI 이동 중지
                _navMeshAgent.isStopped = true;

                // 20유닛의 반지름을 가진 가상의 구를 그렸을 때 구와 겹치는 모든 콜라이더를 가져옴
                // 단, _whatIsTarget 레이어를 가진 콜라이더만 가져오도록 필터링
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, _whatIsTarget);

                // 모든 콜라이더를 순회하면서 살아 있는 LivingEntity 찾기
                for (int i = 0; i < colliders.Length; i++)
                {
                    // 콜라이더로부터 LivingEntity 컴포넌트 가져오기
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    // LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아 있다면
                    if (livingEntity != null && !livingEntity.Dead)
                    {
                        // 추적 대상을 해당 LivingEntity로 설정
                        _targetEntity = livingEntity;

                        break;
                    }
                }
            }
            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!Dead)
        {
            _hitEffect.transform.position = hitPoint;
            _hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            _hitEffect.Play();

            Managers.Sound.Play("Zombie Damage");
        }
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
    public override void Die()
    {
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        Collider[] zombieColliders = GetComponents<Collider>();
        for (int i = 0; i < zombieColliders.Length; i++)
        {
            zombieColliders[i].enabled = false;
        }

        _navMeshAgent.isStopped = true;
        _navMeshAgent.enabled = false;

        _zombieAnimator.SetTrigger("Die");
        Managers.Sound.Play("Zombie Die");
    }

    void OnTriggerStay(Collider other)
    {
        // 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행
        if (!Dead && Time.time >= _lastAttackTime + _timeBetAttack)
        {
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();

            if (attackTarget != null && attackTarget == _targetEntity)
            {
                _lastAttackTime = Time.time;

                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                attackTarget.OnDamage(_damage, hitPoint, hitNormal);
            }
        }
    }
}
