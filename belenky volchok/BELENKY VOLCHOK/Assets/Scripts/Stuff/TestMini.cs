using UnityEngine;

public class TestMinigame : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionName = "Тестовая мини-игра";
    [SerializeField] private string minigameMessage = "Запущена тестовая мини-игра!";
    
    public void Interact()
    {
        Debug.Log($"{minigameMessage} Объект: {gameObject.name}");
        Debug.Log("Мини-игра работает! Здесь будет ваша логика.");
        
        // Здесь можно добавить дополнительную логику
        // Например: загрузка сцены, активация UI и т.д.
    }
    
    public string GetInteractionName()
    {
        return interactionName;
    }
}
