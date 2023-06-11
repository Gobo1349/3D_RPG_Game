using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;
using DG.Tweening;
using System.Linq;

// ВОПРОСЫ - РАЗОБРАТЬСЯ С ACTION, СОБЫТИЯМИ, ТВИНАМИ, _agent!!!

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    private CharacterController _characterController;

    private float _gravity = 20.0f; // условная гравитация
    private Vector3 _moveDirection = Vector3.zero; // вектор движения 

    private bool _isRunning = false;

    private const float PLAYER_DONT_MOVE_SPEED = 0.01F;

    public float speed = 5.0f; // скорость 
    public float rotationSpeed = 240f; // угол поворота 

    // COMBAT SYSTEM 
    [SerializeField] // healthbar
    private ImageBar _healthbar;

    private Collider _collider; // коллайдер игрока

    // событие смерти игрока - враги смогут подписаться на него 
    public static System.Action OnDeath;

    // переменные для вычисления времени проигрывания анимации атак 
    private float _superAttackTime;
    private float _attackTime;

    // переменная текущей жизни
    private float _health;

    [SerializeField]
    private float _maxHealth;

    // задаем коллайдер оружия - чтобы могли обратиться к оружию 
    [SerializeField]
    private BoxCollider _weaponCollider;

    // проверка в данный момент времени - находится ли персонаж в состоянии атаки 
    private bool _isAttacking = false;

    // константа вероятности использования суператаки 
    private const float SUPER_ATTACK_PROBABILITY = 0.3F;

    // задаем твин для атаки 
    private Tween _attackTween; // для анимации 

    // погиб ли игрок 
    private bool _isDead = false; 

    // инфа для врагов - жив ли герой 
    public bool IsDead
    {
        get { return _isDead; }
    }

    [SerializeField]
    private GameObject _swordTrail; // след от взмаха меча 

    [SerializeField]
    private ParticleSystem _bloodParticle; // частицы крови при ударе 
    private void Awake()
    {
        // узнаем, сколько времени проигрываются атаки - для твинов
        _superAttackTime = _animator.runtimeAnimatorController.animationClips.ToList().Find(a => a.name == "2Hand-Sword-Attack6").length;
        _attackTime = _animator.runtimeAnimatorController.animationClips.ToList().Find(a => a.name == "2Hand-Sword-Attack1").length;

        _characterController = GetComponent<CharacterController>(); 
        _collider = GetComponent<Collider>();

        _swordTrail.SetActive(false); // В начале игры след выключен 

        // частицы крови
        _bloodParticle = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
        // выключаем оружие, чтобы игрок НЕ во время атаки не мог ранить врагов при коллизии
        _weaponCollider.enabled = false;
        _health = _maxHealth; // здоровье полное 
    }

    void Update()
    {
        // проверка что герой умер 
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

        // теперь узнаем куда направлена камера в плоскости X-Z и определяем направление движения персонажа в ту сторону, куда смотрит джойстик 
        Vector3 camForward_Dir = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized; // Покомпонентное умножение двух vector'oв
        Vector3 move = v * camForward_Dir + h * Camera.main.transform.right; // узнаем как сильно и куда наклонен джойстик 

        if (move.magnitude > 1) // если вектор больше 1 
        {
            move.Normalize(); // ограничиваем от 0 до 1 наклон джойстика - больше 1 он не будет
        }

        // приводим координаты наклона джойстика из глобальных в локальные 
        move = transform.InverseTransformDirection(move); // Преобразовывает direction (направление) из глобальной в локальную систему координат

        // узнаем угол, на который должен повернуться персонаж вслед за джойстиком 
        float turnAmount = Mathf.Atan2(move.x, move.y);

        // поворачиваем персонажа на заданный угол со скоростью указанной указанной выше - скорость поворота 
        transform.Rotate(0, turnAmount * rotationSpeed * Time.deltaTime, 0);

        // проверим, что персонаж на земле - из-за этой проверки была ошибка, почему - пока хз
       // if (_characterController.isGrounded)
        //{
            // умножаем вектор направления персонажа на вектор направления джойстика
            _moveDirection = transform.forward * move.magnitude;
            _moveDirection *= speed;
        //}

        // имитируем гравитацию персонажа в игровом пространстве 
        _moveDirection.y -= _gravity * Time.deltaTime;

        // поправка - чтобы не мог одновременно атаковать и ходить  
       // if (!_isAttacking)
       // {
            // теперь двигаем персонажа 
            _characterController.Move(_moveDirection * Time.deltaTime);
       // }

        // теперь нужно узнать скорость персонажа в игре для анимаций 
        Vector3 horVelocity = _characterController.velocity;

        // получить скорость движения персонажа в осях X - Z
        horVelocity = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z);

        // узнаем длину вектора скорости - для анимаций 
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

    // боевка 
    public void Attack()
    {
        if (!_isAttacking && !_isDead) // если не было атаки и если герой живой 
        {
            _isAttacking = true; // теперь атакует 
            _weaponCollider.enabled = true; // включаем оружие 

            _swordTrail.SetActive(true); // При атаке след включается  

            // запускаем аниматор с рандомным выбором триггера между обычной и супер атакой 
            // это нужно чтобы мы знали какую анимацию мы будем дергать 
            string attackAnim = Random.value > SUPER_ATTACK_PROBABILITY ?
                                            AttackType.Attack.ToString() : AttackType.SuperAttack.ToString();

            // дергаем триггер атаки для анимации атаки 
            _animator.SetTrigger(attackAnim);

            // задаем время текущей атаки для корутины (?) dotween 
            float thisAttackTime = attackAnim == AttackType.Attack.ToString() ?
                                    _attackTime : _superAttackTime;

            // запускаем твин (?)
            _attackTween = DOVirtual.DelayedCall(thisAttackTime, // сколько времени должна проигрываться анимация до того, как надо выключить коллайдер и сказать что атака закончена 
                                                delegate
                                                {
                                                    _isAttacking = false;
                                                    _weaponCollider.enabled = false;
                                                    _swordTrail.SetActive(false); // В конце атаки след выключается 
                                                    _animator.SetTrigger("Idle");
                                                }
                                                    );
        }
    }

    // убиваем твины - если объект будет отключаться 
    private void OnDisable()
    {
        _attackTween.Kill();
    }

    // чтобы игрок смог получить урон 
    public void GetDamage(float value)
    {
        // отнять от текущего здоровья урон врага и ограничить значение до 0 (ХП не может быть меньше 0)
        _health = Mathf.Clamp(_health - value, 0, _health);

        // нужно уменьшить полосу здоровья в интерфейсе 
        _healthbar.SetValue(_health, _maxHealth);

        if (_health <= 0)
        {
            Die();
        }

        BloodEffect(); // эффект крови
    }

    public void Heal(float value)
    {
        _health = Mathf.Clamp(_health + value, 0, _maxHealth);

        // нужно увеличить полосу здоровья в интерфейсе 
        _healthbar.SetValue(_health, _maxHealth);
    }
    private void Die()
    {
        _isDead = true;
        _attackTween.Kill();
        _collider.enabled = false;

        _animator.SetTrigger("Die"); // анимация смерти 

        // "кричим" - событие - герой умер 
        if (OnDeath != null) // хоть кто то подписан - тогда кричим 
        {
            OnDeath();
        }    
    }

    private void BloodEffect() // эффект брызг крови (ахаха)
    {
        _bloodParticle.Stop(); // если уже были частицы - выключаем их 
        _bloodParticle.Play(); // запускаем частицы
    }
}

public enum AttackType
{
    Attack, 
    SuperAttack
}
