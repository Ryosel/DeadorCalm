using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : LivingEntity
{
    [SerializeField]
    LayerMask _whatIsTarget; // ���� ��� ���̾�

    LivingEntity _targetEntity; // ���� ���
    NavMeshAgent _navMeshAgent; // ��� ��� AI ������Ʈ

    [SerializeField]
    ParticleSystem _hitEffect; // �ǰ� �� ����� ��ƼŬ ȿ��

    Animator _zombieAnimator; // �ִϸ����� ������Ʈ
    Renderer _zombieRenderer; // ������ ������Ʈ

    [SerializeField]
    float _damage = 20f; // ���ݷ�
    [SerializeField]
    float _timeBetAttack = 0.5f; // ���� ����
    float _lastAttackTime; // ������ ���� ����

    // ������ ����� �����ϴ��� �˷��ִ� ������Ƽ
    bool HasTarget
    {
        get
        {
            // ������ ����� �����ϰ�, ����� ������� �ʾҴٸ� true
            if (_targetEntity != null && !_targetEntity.Dead)
            {
                return true;
            }

            // �׷��� �ʴٸ� false
            return false;
        }
    }

    private void Awake()
    {
        // �ʱ�ȭ
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _zombieAnimator = GetComponent<Animator>();
        _zombieRenderer = GetComponentInChildren<Renderer>();
    }

    // ���� AI�� �ʱ� ������ �����ϴ� �¾� �޼���
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

        // ���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� ���� ��ƾ ����
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        // ���� ����� ���� ���ο� ���� �ٸ� �ִϸ��̼� ���
        _zombieAnimator.SetBool("HasTarget", HasTarget);
    }

    // �ֱ������� ������ ����� ��ġ�� ã�� ��� ����
    IEnumerator UpdatePath()
    {
        // ��� �ִ� ���� ���� ����
        while (!Dead)
        {
            if (HasTarget)
            {
                // ���� ��� ���� : ��θ� �����ϰ� AI �̵��� ��� ����
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(_targetEntity.transform.position);
            }
            else
            {
                // ���� ��� ���� : AI �̵� ����
                _navMeshAgent.isStopped = true;

                // 20������ �������� ���� ������ ���� �׷��� �� ���� ��ġ�� ��� �ݶ��̴��� ������
                // ��, _whatIsTarget ���̾ ���� �ݶ��̴��� ���������� ���͸�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, _whatIsTarget);

                // ��� �ݶ��̴��� ��ȸ�ϸ鼭 ��� �ִ� LivingEntity ã��
                for (int i = 0; i < colliders.Length; i++)
                {
                    // �ݶ��̴��κ��� LivingEntity ������Ʈ ��������
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    // LivingEntity ������Ʈ�� �����ϸ�, �ش� LivingEntity�� ��� �ִٸ�
                    if (livingEntity != null && !livingEntity.Dead)
                    {
                        // ���� ����� �ش� LivingEntity�� ����
                        _targetEntity = livingEntity;

                        break;
                    }
                }
            }
            // 0.25�� �ֱ�� ó�� �ݺ�
            yield return new WaitForSeconds(0.25f);
        }
    }

    // �������� �Ծ��� �� ������ ó��
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!Dead)
        {
            _hitEffect.transform.position = hitPoint;
            _hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            _hitEffect.Play();

            Managers.Sound.Play("Zombie Damage");
        }
        // LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // ��� ó��
    public override void Die()
    {
        // LivingEntity�� Die()�� �����Ͽ� �⺻ ��� ó�� ����
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
        // Ʈ���� �浹�� ���� ���� ������Ʈ�� ���� ����̶�� ���� ����
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
