using System;
using TUDarmstadt.SeriousGames.MoodleQuizParser;

public class ActionManager
{
    /// <summary>
    /// A hook for the user to provide their own data saving logic.
    /// Passes the Question ID (int) and the Result (QuizResult).
    /// </summary>
    public static Action<int, QuizResult> OnSaveResult;

    /// <summary>
    /// A hook for the user to connect to their own event or message system.
    /// Passes the Result (QuizResult).
    /// </summary>
    public static Action<QuizResult> OnQuizGraded;

    /// <summary>
    /// A hook for the user to connect to their Audiosystem.
    /// </summary>
    public static Action OnButtonPressed;

    /// <summary>
    /// A hook for the user to connect to their UIManager.
    /// </summary>
    public static Action OnCloseView;
}
