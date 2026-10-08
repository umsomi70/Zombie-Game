   using UnityEngine;

   public class GameManager : MonoBehaviour
   {
       public int score;
       public int lives = 3;

       public void AddScore(int amount) { score += amount; Debug.Log("Score: " + score); }
       public void AddLife() { lives++; Debug.Log("Lives: " + lives); }
   }