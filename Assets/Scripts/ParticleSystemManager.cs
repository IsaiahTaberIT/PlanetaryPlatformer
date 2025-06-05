using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleSystemManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Object;
    public ParticleSystem particle;
    public ParticleSystem.Particle[] Particles;
    public Vector3 GravityDirection;

 

    void Start()
    {
        float normalgravity = 0;

        GameObject logic = GameObject.FindGameObjectWithTag("Logic");
        if (logic != null)
        {
            if(logic.TryGetComponent<GameLogicScript>(out GameLogicScript gameLogicScript))
            {
                normalgravity = gameLogicScript.NormalGravity;

            }


        }
        

        particle = GetComponent<ParticleSystem>();
        GravityDirection = GameLogicScript.GravityDirection(transform.position,Vector3.down * normalgravity).Gravity;




    }

    // Update is called once per frame
    void Update()
    {
        Particles = new ParticleSystem.Particle[particle.main.maxParticles];
        particle.GetParticles(Particles);
    
        for (int i = 0; i < Particles.Length; i++)
        {
            Particles[i].velocity += GravityDirection * Time.deltaTime;
        }
        particle.SetParticles(Particles);
    }
}
