using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;
using DG.Tweening;
using System.Linq;

// ������� - ����������� � ACTION, ���������, �������, _agent!!!

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    private CharacterController _characterController;

    private float _gravity = 20.0f; // �������� ����������
    private Vector3 _moveDirection = Vector3.zero; // ������ �������� 

    private bool _isRunning = false;

    private const float PLAYER_DONT_MOVE_SPEED = 0.01F;

    public float speed = 5.0f; // �������� 
    public float rotationSpeed = 240f; // ���� �������� 

    // COMBAT SYSTEM 
    [SerializeField] // healthbar
    private ImageBar _healthbar;

    private Collider _collider; // ��������� ������

    // ������� ������ ������ - ����� ������ ����������� �� ���� 
    public static System.Action OnDeath;

    // ���������� ��� ���������� ������� ������������ �������� ���� 
    private float _superAttackTime;
    private float _attackTime;

    // ���������� ������� �����
    private float _health;

    [SerializeField]
    private float _maxHealth;

    // ������ ��������� ������ - ����� ����� ���������� � ������ 
    [SerializeField]
    private BoxCollider _weaponCollider;

    // �������� � ������ ������ ������� - ��������� �� �������� � ��������� ����� 
    private bool _isAttacking = false;

    // ��������� ����������� ������������� ���������� 
    private const float SUPER_ATTACK_PROBABILITY = 0.3F;

    // ������ ���� ��� ����� 
    private Tween _attackTween; // ��� �������� 

    // ����� �� ����� 
    private bool _isDead = false; 

    // ���� ��� ������ - ��� �� ����� 
    public bool IsDead
    {
        get { return _isDead; }
    }

    [SerializeField]
    private GameObject _swordTrail; // ���� �� ������ ���� 

    [SerializeField]
    private ParticleSystem _bloodParticle; // ������� ����� ��� ����� 
    private void Awake()
    {
        // ������, ������� ������� ������������� ����� - ��� ������
        _superAttackTime = _animator.runtimeAnimatorController.animationClips.ToList().Find(a => a.name == "2Hand-Sword-Attack6").length;
        _attackTime = _animator.runtimeAnimatorController.animationClips.ToList().Find(a => a.name == "2Hand-Sword-Attack1").length;

        _characterController = GetComponent<CharacterController>(); 
        _collider = GetComponent<Collider>();

        _swordTrail.SetActive(false); // � ������ ���� ���� �������� 

        // ������� �����
        _bloodParticle = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        // ��������� ������, ����� ����� �� �� ����� ����� �� ��� ������ ������ ��� ��������
        _weaponCollider.enabled = false;
        _health = _maxHealth; // �������� ������ 
    }

    void Update()
    {
        // �������� ��� ����� ���� 
        if (_isDead)
        {
            return;
        }
        Move();
    }

    private void Move()
    {
        float h = CrossPlatformInputManager.GetAxis("Horizontal"); // Returns the value of the virtual axis
        float v = CrossPlatformInputManager.GetAxis("Vertical");

        // ������ ������ ���� ���������� ������ � ��������� X-Z � ���������� ����������� �������� ��������� � �� �������, ���� ������� �������� 
        Vector3 camForward_Dir = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized; // �������������� ��������� ���� vector'o�
        Vector3 move = v * camForward_Dir + h * Camera.main.transform.right; // ������ ��� ������ � ���� �������� �������� 

        if (move.magnitude > 1) // ���� ������ ������ 1 
        {
            move.Normalize(); // ������������ �� 0 �� 1 ������ ��������� - ������ 1 �� �� �����
        }

        // �������� ���������� ������� ��������� �� ���������� � ��������� 
        move = transform.InverseTransformDirection(move); // ��������������� direction (�����������) �� ���������� � ��������� ������� ���������

        // ������ ����, �� ������� ������ ����������� �������� ����� �� ���������� 
        float turnAmount = Mathf.Atan2(move.x, move.y);

        // ������������ ��������� �� �������� ���� �� ��������� ��������� ��������� ���� - �������� �������� 
        transform.Rotate(0, turnAmount * rotationSpeed * Time.deltaTime, 0);

        // ��������, ��� �������� �� ����� - ��-�� ���� �������� ���� ������, ������ - ���� ��
       // if (_characterController.isGrounded)
        //{
            // �������� ������ ����������� ��������� �� ������ ����������� ���������
            _moveDirection = transform.forward * move.magnitude;
            _moveDirection *= speed;
        //}

        // ��������� ���������� ��������� � ������� ������������ 
        _moveDirection.y -= _gravity * Time.deltaTime;

        // �������� - ����� �� ��� ������������ ��������� � ������  
       // if (!_isAttacking)
       // {
            // ������ ������� ��������� 
            _characterController.Move(_moveDirection * Time.deltaTime);
       // }

        // ������ ����� ������ �������� ��������� � ���� ��� �������� 
        Vector3 horVelocity = _characterController.velocity;

        // �������� �������� �������� ��������� � ���� X - Z
        horVelocity = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z);

        // ������ ����� ������� �������� - ��� �������� 
        float horSpeed = horVelocity.magnitude;
        //_animator.SetFloat("Speed", horSpeed);
        if (horSpeed > 0 && _isRunning == false)
        {
            _animator.SetTrigger("Walk");
            _isRunning = true;
        }

        if (horSpeed < PLAYER_DONT_MOVE_SPEED && _isRunning == true)
        {
            _animator.SetTrigger("Idle");
            _isRunning = false;
        }
    }

    // ������ 
    public void Attack()
    {
        if (!_isAttacking && !_isDead) // ���� �� ���� ����� � ���� ����� ����� 
        {
            _isAttacking = true; // ������ ������� 
            _weaponCollider.enabled = true; // �������� ������ 

            _swordTrail.SetActive(true); // ��� ����� ���� ����������  

            // ��������� �������� � ��������� ������� �������� ����� ������� � ����� ������ 
            // ��� ����� ����� �� ����� ����� �������� �� ����� ������� 
            string attackAnim = Random.value > SUPER_ATTACK_PROBABILITY ?
                                            AttackType.Attack.ToString() : AttackType.SuperAttack.ToString();

            // ������� ������� ����� ��� �������� ����� 
            _animator.SetTrigger(attackAnim);

            // ������ ����� ������� ����� ��� �������� (?) dotween 
            float thisAttackTime = attackAnim == AttackType.Attack.ToString() ?
                                    _attackTime : _superAttackTime;

            // ��������� ���� (?)
            _attackTween = DOVirtual.DelayedCall(thisAttackTime, // ������� ������� ������ ������������� �������� �� ����, ��� ���� ��������� ��������� � ������� ��� ����� ��������� 
                                                delegate
                                                {
                                                    _isAttacking = false;
                                                    _weaponCollider.enabled = false;
                                                    _swordTrail.SetActive(false); // � ����� ����� ���� ����������� 
                                                    _animator.SetTrigger("Idle");
                                                }
                                                    );
        }
    }

    // ������� ����� - ���� ������ ����� ����������� 
    private void OnDisable()
    {
        _attackTween.Kill();
    }

    // ����� ����� ���� �������� ���� 
    public void GetDamage(float value)
    {
        // ������ �� �������� �������� ���� ����� � ���������� �������� �� 0 (�� �� ����� ���� ������ 0)
        _health = Mathf.Clamp(_health - value, 0, _health);

        // ����� ��������� ������ �������� � ���������� 
        _healthbar.SetValue(_health, _maxHealth);

        if (_health <= 0)
        {
            Die();
        }

        BloodEffect(); // ������ �����
    }

    public void Heal(float value)
    {
        _health = Mathf.Clamp(_health + value, 0, _maxHealth);

        // ����� ��������� ������ �������� � ���������� 
        _healthbar.SetValue(_health, _maxHealth);
    }
    private void Die()
    {
        _isDead = true;
        _attackTween.Kill();
        _collider.enabled = false;

        _animator.SetTrigger("Die"); // �������� ������ 

        // "������" - ������� - ����� ���� 
        if (OnDeath != null) // ���� ��� �� �������� - ����� ������ 
        {
            OnDeath();
        }    
    }

    private void BloodEffect() // ������ ����� ����� (�����)
    {
        _bloodParticle.Stop(); // ���� ��� ���� ������� - ��������� �� 
        _bloodParticle.Play(); // ��������� �������
    }
}

public enum AttackType
{
    Attack, 
    SuperAttack
}
