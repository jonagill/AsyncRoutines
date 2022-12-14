namespace AsyncRoutines 
{
    public enum UpdatePhase
    {
        Update = 0,
        PostUpdate = 100,
        FixedUpdate = 200,
        LateUpdate = 300,
        PreRender = 400,
        EndOfFrame = 999,
    }
}
