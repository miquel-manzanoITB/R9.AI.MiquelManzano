using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    [SerializeField]
    private TextMeshPro hpText; // Arrastra un Text de UI aquí (opcional)

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        UpdateUI();
        Debug.Log("Player HP: " + currentHP);

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("¡Player muerto!");
        gameObject.SetActive(false);
        // Aquí puedes cargar una escena de Game Over:
        // SceneManager.LoadScene("GameOver");
    }

    void UpdateUI()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHP;
    }
}