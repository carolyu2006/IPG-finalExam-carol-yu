namespace Template;

// service locator
// static class that contains static references to non static variables
// static class - only one of them, globally accessible. 

public static class ServiceLocator
{
    public static Game1 Game1;

    public static GameState GameState;

    public static Input Input;
    // Time delta (seconds) updated each frame by Game1
    public static float DeltaSeconds = 1f / 60f;
}