using Gravitons.UI.Modal;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Sprite bgImage;
    public List<Button> btns = new List<Button>();

    public Sprite[] puzzles;
    public List<Sprite> gamePuzzles = new List<Sprite>();

    private bool firstGuess, secondGuess;
    private int countGuesses;
    private int countCorrectGuesses;
    private int gameGuesses;

    private string firstGuessPuzzle,secondGuessPuzzle;
    private int firstGuessIndex,secondGuessIndex;
    private void Start()
    {
        GetButtons();
        AddListeners();
        AddGamePuzzles();
        Shuffle(gamePuzzles);
        gameGuesses = gamePuzzles.Count / 2;
    }
    void GetButtons()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("PuzzleButton");
        for (int i = 0; i < objects.Length; i++)
        {
            btns.Add(objects[i].GetComponent<Button>());
            btns[i].image.sprite = bgImage;
        }
    }

    void AddGamePuzzles()
    {
        int looper = btns.Count;
        int index = 0;
        for ( int i = 0; i < looper; i++ )
        {
            if(index == looper / 2)
            {
                index = 0;
            }
            gamePuzzles.Add(puzzles[index]);
            index++;
        }
    }

    void AddListeners()
    {
        foreach ( Button button in btns )
        {
            button.onClick.AddListener(() => PickPuzzle());
        }
    }
    public void PickPuzzle()
    { 
        
        if(!firstGuess)
        {
            firstGuess = true;
            

            firstGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
            firstGuessPuzzle = gamePuzzles[firstGuessIndex].name;
            btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
            btns[firstGuessIndex].interactable=false;
        }
        else if (!secondGuess)
        {
            secondGuess = true;
           

            secondGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
            secondGuessPuzzle = gamePuzzles[secondGuessIndex].name;
            btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];
            btns[secondGuessIndex].interactable = false;
            StartCoroutine(CheckIfThePuzzleMatch());
        }

    }
    IEnumerator CheckIfThePuzzleMatch()
    {
        countGuesses++;
        yield return new WaitForSeconds(1f);
        if (firstGuessPuzzle == secondGuessPuzzle)
        {
            yield return new WaitForSeconds(1f);
            btns[firstGuessIndex].interactable = false;
            btns[secondGuessIndex].interactable = false;

            btns[firstGuessIndex].image.color = new Color(0,0,0,0);
            btns[secondGuessIndex].image.color = new Color(0, 0, 0,0);
            CheckIfGameIsFinished();
        }
        else
        {
            yield return new WaitForSeconds(.5f);
            btns[firstGuessIndex].interactable = true;
            btns[secondGuessIndex].interactable = true;
            btns[firstGuessIndex].image.sprite = bgImage;
            btns[secondGuessIndex].image.sprite = bgImage;
        }
        yield return new WaitForSeconds(.5f);

        firstGuess = secondGuess = false;
    }
    void CheckIfGameIsFinished()
    {
        countCorrectGuesses++;
        if(countCorrectGuesses == gameGuesses)
        {
            Debug.Log(countGuesses);
            ModalManager.Show("végeztél",
                $"{countGuesses}db próbálkozásból",
                new[]
                {
                    new ModalButton() { Text = "OK", Callback = BackToMainGame }
                }
            );
        }
    }
    void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIdx = Random.Range(0, list.Count);
            list[i] = list[randomIdx];
            list[randomIdx] = temp;
        }
    }

    void BackToMainGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex -1);
    }
}
