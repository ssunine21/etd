using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.View.ViewEnemy;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.InGame.Controller.ControllerEnemy
{
    public abstract class ControllerEnemy : IDamageable
    {
        public EnemyType Type { get; set; }
        public Vector2 Position { get; set; }
        public double MaxHp { get; set; }
        public double CurrHp { get; set; }
        public double Power { get; set; }
        public float AttackRange { get; set; }
        public float AttackSpeed { get; set; }
        public float MoveSpeed { get; set; }
        public bool IsBoss { get; set; }
        public bool IsActive => viewEnemy.isActiveAndEnabled;
        public View.View View => viewEnemy;
        public bool IsInView { get; protected set; }

        public readonly ViewEnemy viewEnemy;
        
        protected readonly CancellationTokenSource cts;
        protected Vector2 DirectionToMainUnit;
        protected bool IsAttackable { get; set; }
        
        private const float RotateSpeed = 25f;

        private Vector3 _rotate;
        private Vector3 _rotateDirection;

        public ControllerMainUnit MainUnit
        {
            get
            {
                if(_mainUnit == null) _mainUnit = MainUnitManager.Instance.MainUnitController;
                return _mainUnit;
            }
        }
        private ControllerMainUnit _mainUnit;
        
        protected ControllerEnemy(CancellationTokenSource cts, ViewEnemy view, EnemyType type)
        {
            this.cts = cts;
            Type = type;        
            viewEnemy = view;
            IsBoss = viewEnemy.IsBoss;
            
            SetRandomRotate();
        }

        public abstract UniTaskVoid MainTask();

        public void Init(EnemyDifficultyInfo info)
        {
            MaxHp = DataController.Instance.difficulty.GetEnemyHp(info, IsBoss);
            Power = DataController.Instance.difficulty.GetEnemyPower(info, IsBoss);
            
            CurrHp = MaxHp;

            MoveSpeed = DataController.Instance.enemy.GetMoveSpeed(Type);
            AttackRange = DataController.Instance.enemy.GetAttackRange(Type);
            AttackSpeed = DataController.Instance.enemy.GetAttackSpeed(Type);
            
            viewEnemy.SetActive(true);
            viewEnemy.UpdateHp(GetHpScale());
        }

        public virtual void Damaged(double damage, bool isCritical)
        {
            if (!GameManager.Instance.IsPlaying) return;
            if (!ControllerCanvasTest.IsEnemyInvincibility)
            {
                if (ReduceHealthAndCheckDeath(damage))
                {
                    Die();
                }
                else
                {
                    viewEnemy.UpdateHp(GetHpScale());
                }
            }
            
            EnemyManager.Instance.onBindDamagedEnemy?.Invoke(damage);
            TextManager.Instance.ShowDamage(damage, Position, isCritical);
        }

        private void Die()
        {
            viewEnemy.SetActive(false);
            EnemyManager.Instance.onBindDieEnemy?.Invoke(this);
            DataController.Instance.quest.Count(QuestType.KillEnemy);
            DataController.Instance.mission.Count(MissionType.KillEnemy);
        }
        
        protected void Rotate()
        {
            _rotate += _rotateDirection * (RotateSpeed * Time.deltaTime);
        }

        private void Tracking(Vector2 targetPosition)
        {
            DirectionToMainUnit = (targetPosition - Position).normalized;
            Position += DirectionToMainUnit * (MoveSpeed * Time.deltaTime);
        }

        protected void TrackingToMainUnit()
        {
            if (!GameManager.Instance.IsPlaying)
            {
                IsAttackable = false;
                return;
            }
            
            var distanceToTarget =  Vector2.Distance(Position, MainUnit.Position);

            if(IsInView)
            {
                if (distanceToTarget <= (AttackRange + MainUnit.ColliderRange))
                {
                    IsAttackable = true;
                    return;
                }
            }

            IsAttackable = false;
            Tracking(MainUnit.Position);
        }

        protected virtual void UpdateView()
        {
            viewEnemy.UpdatePosition(Position);
            viewEnemy.Rotate(_rotate);
        }
    
        private float GetHpScale()
        {
            return Mathf.Clamp01((float)CurrHp / (float)MaxHp);
        }

        private void SetRandomRotate()
        {
            _rotateDirection = Random.Range(0, 2) == 0 ? Vector3.back : Vector3.forward;
            _rotate += _rotateDirection * Random.Range(0f, 360f);
        }

        private bool ReduceHealthAndCheckDeath(double damage)
        {
            CurrHp -= damage;
            return CurrHp <= 0;
        }
    }
}