using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



    public class IAscript : MonoBehaviour
    {
        public NavMeshAgent agent;
        public CharacterController character;
        public GameObject[] player;

        public enum State
        {
            PATROL,
            CHASE,
            ATTACK
        }

        public State state;
        private bool isAlive = true;

        public GameObject[] waypoints;
        private int waypointInd;
        public float patrolSpeed = 0.01f;

        public float chaseSpeed = 0.02f;
        private float chaseDistance = 30f;
        public GameObject target;

        private float attackDistance = 10f;
        public bool targetPlayer = false;

        private Animator anim;

        public GameObject bullet;
        public float speedBullet = 50f;
        private float timeBetweenBullets = 3f;

        // Start is called before the first frame update
        void Start()
        {

            agent = GetComponent<NavMeshAgent>();
            character = GetComponent<CharacterController>();

            //agent.updatePosition = true;
            //agent.updateRotation = false;

            waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
            waypointInd = Random.Range(0, waypoints.Length);
            target = waypoints[waypointInd];

            player = GameObject.FindGameObjectsWithTag("Player");

            state = State.PATROL;

            anim = GetComponent<Animator>();

            StartCoroutine(FSM());
            
        }

        
        IEnumerator FSM()
        {
            while (isAlive){
                switch (state)
                {
                case State.PATROL:
                    patrol();
                    break;
                case State.CHASE:
                    chase();
                    break;
                case State.ATTACK:
                    attack();
                    break;
                }
            yield return null;
            }

        }
        
        void checkPlayer(){
            
            foreach (GameObject Player in player)
            {
                
                if ((Vector3.Distance(transform.position, Player.transform.position) < chaseDistance))
                {
                    target = player[0];
                    if(Vector3.Distance(transform.position, Player.transform.position) < Vector3.Distance(transform.position, target.transform.position))
                    {
                        target = Player;
                    }
                    targetPlayer = true;
                    state = State.CHASE;
                }
                else
                {
                    targetPlayer = false;
                }
            }
        }

        void patrol()
        {
            agent.speed = patrolSpeed;
            if (Vector3.Distance(gameObject.transform.position, target.transform.position) >= 2)
            {
                agent.SetDestination(target.transform.position);
                character.Move(agent.desiredVelocity);
            }
            else if (Vector3.Distance(transform.position, target.transform.position) <= 2)
            {
                waypointInd = Random.Range(0, waypoints.Length);
                target = waypoints[waypointInd];
            }
            else
            {
                character.Move(Vector3.zero);
            }
           
        }

        void chase()
        {
            agent.speed = chaseSpeed;
            if (targetPlayer)
            {
                agent.SetDestination(target.transform.position);
                character.Move(agent.desiredVelocity);

                if (Vector3.Distance(transform.position, target.transform.position) <= attackDistance)
                {
                    state = State.ATTACK;
                }
            }
            else
            {
                waypointInd = Random.Range(0, waypoints.Length);
                target = waypoints[waypointInd];
                targetPlayer = false;
                state = State.PATROL;
            }

            
        }

        void attack()
        {
            if (Vector3.Distance(transform.position, target.transform.position) > attackDistance)
            {
                checkPlayer();
                agent.isStopped = false;
                state = State.CHASE;
            }
            else
            {
                agent.speed = 0f;
                agent.isStopped = true;
                Vector3 direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);

            }
        }

        void shoot(Vector3 PlayerPos)
        {
            float Rx = Random.Range(-10, 10);
            float Ry = Random.Range(-8, 10);
            float Rz = Random.Range(-10, 10);

            GameObject bulletClone = Instantiate(bullet, new Vector3(transform.position.x,transform.position.y + 1.7f, transform.position.z), transform.rotation);
            Rigidbody bulletCloneRigidbody = bulletClone.GetComponent<Rigidbody>();
            bulletCloneRigidbody.velocity = bulletClone.transform.forward * speedBullet + new Vector3(Rx, Ry, Rz);

            Destroy(bulletClone,2f);
        }


        void FaceTarget()
        {
            var turnTowardNavSteeringTarget = agent.steeringTarget;
     
            Vector3 direction = (turnTowardNavSteeringTarget - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
        }

        // Update is called once per frame
        void Update()
        {
            player = GameObject.FindGameObjectsWithTag("Player");
            if(player != null && state != State.ATTACK)
            {
                checkPlayer();
            }
            if(state != State.ATTACK)
            {
                FaceTarget();
            }
            else
            {
                timeBetweenBullets -= Time.deltaTime;
                if (timeBetweenBullets <= 0)
                {
                    Vector3 PlayerPos = target.transform.position;
                    shoot(PlayerPos);
                    timeBetweenBullets = 3f;
                }
            }      

            anim.SetFloat("Speed", agent.velocity.magnitude * 50);
        }
    }