﻿//Version 1.99 (26.02.2018)

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Text;
using System.Xml;
using System.IO;
using TriviaQuizGame.Types;
using System.Collections.Generic;

namespace TriviaQuizGame
{
    /// <summary>
    /// This script controls the game, starting it, following game progress, and finishing it with game over.
    /// </summary>
    public class TQGGameController : MonoBehaviour
    {
        // Holds the current event system
        internal EventSystem eventSystem;

        // This is used when parsing the Xml info
        internal XmlNodeList xmlRecords;

        // Holds the dynamic XML component which imports quetsions from an online or local address
        internal TQGDynamicXML dynamicXML;

        // Holds the name of the category loaded into this quiz, if it exists
        internal string currentCategory;

        [Header("<Player Options>")]
        [Tooltip("A list of the players in the game. Each player can be assigned a name, a score text, lives and lives bar. You must have at least one player in the list in order to play the game. You don't need to assign all fields. For example, a player may have a name with no lives bar and it will work fine.")]
        public Player[] players;

        //The current turn of the player. 0 means it's player 1's turn to play, 1 means it's player 2's turn, etc
        internal int currentPlayer = 0;

        // Is this game played in hot-seat mode? This mode lets each player answer a question in turn.
        internal bool playInTurns = true;

        [Tooltip("The number of lives each player has. You lose a life if time runs out, or you answer wrongly too many times")]
        public float lives = 3;

        // The width of a single life in the lives bar. This is calculated from the total width of a life bar divided by the number of lives
        internal float livesBarWidth = 128;

        [Tooltip("The number of players participating in the match. This number cannot be larger than the total number of players list ( The ones that you assign a scoreText/livesBar to )")]
        public int numberOfPlayers = 4;

        internal RectTransform playersObject;
        [Header("<Question Options>")]

        [Tooltip("The object that displays the current question")]
        public Transform questionObject;

        // The image and video objects inside the question object
        internal RectTransform imageObject;
        internal GameObject videoObject;

        [Tooltip("A list of all possible questions in the game. Each question has a number of correct/wrong answers, a followup text, a bonus value, time, and can also have an image/video as the background of the question")]
        public Question[] questions;
        internal Question[] questionsTemp;

        [Tooltip("The number of the first question being asked. You can change this to start from a higher question number")]
        public int firstQuestion = 1;

        // The index of the current question being asked. -1 is the index of the first question, 0 the index of the second, and so on
        internal int currentQuestion = -1;

        // Is a question being asked right now?
        internal bool askingQuestion;

        [Tooltip("Randomize the list of questions. Use this if you don't want the questions to appear in the same order every time you play. Combine this with 'sortQuestions' if you want the questions to be randomized within the bonus groups.")]
        public bool randomizeQuestions = true;

        //The buttons that display the possible answers
        internal Transform[] answerObjects;

        [Tooltip("Randomize the display order of answers when a new question is presented")]
        public bool randomizeAnswers = true;

        // The currently selected answer when using the ButtonSelector
        internal int currentAnswer = 0;
        internal GameObject multiChoiceButton;

        // Holds the answers when shuffling them
        internal string[] tempAnswers;

        [Tooltip("Sort the list of questions from lowest bonus to highest bonus and put them into groups. Use this if you want the questions to be displayed from the easiest to the hardest ( The difficulty of a question is decided by the bonus value you give to it )")]
        public bool sortQuestions = true;

        [Tooltip("Prevent a quiz from repeating questions. Once all questions in a quiz have been asked, they will repeat again.")]
        public bool dontRepeatQuestions = true;

        [Tooltip("How many questions from the current bonus group should be asked before moving on to the next group. If we dont sort the questions by bonus groups, then this value is ignored. If there are several players in the game, the value of Questions Per Group will be multiplied by the number of players, so that each one can have a chance to answer a question from the same group before moving on to the next group")]
        public int questionsPerGroup = 2;
        internal int defaultQuestionsPerGroup;
        internal int questionCount = 0;

        // The progress object which shows all the progress tabs and how many questions are left to win
        internal RectTransform progressObject;

        // The progress tab and text objects which show which question we are on. The tab also shows if we answered a question correctly or not
        internal GameObject progressTabObject;
        internal GameObject progressTextObject;

        // The size of the progress tab. This is calculated automatically and used to align the progress bar to the center.
        internal float progressTabSize;

        [Tooltip("Limit the total number of questions asked, regardless of whether we answered correctly or not. Use this if you want to have a strict number of questions asked in the game (ex: 10 questions). If you keep it at 0 the number of questions will not be limited and you will go through all the question groups in the quiz before finishing it")]
        public int questionLimit = 0;

        // The total number of questions we asked. This is used to check if we reached the question limit.
        internal int questionLimitCount = 0;

        // The number of questions we answered correctly. This is used for displaying the result at the end of the game
        internal int correctAnswers = 0;
        internal float wrongAnswers = 0;

        [Tooltip("The maximum number of mistakes allowed. If you make to many mistakes you lose a life and move to the next question")]
        public int maximumMistakes = 2;
        internal int mistakeCount = 0;

        // How many seconds are left before game over
        internal float timeLeft = 10;

        // Is the timer running?
        internal bool timerRunning = false;

        [Tooltip("If we set this time higher than 0, it will override the individual times for each question. The global time does not reset between questions")]
        public float globalTime = 0;

        [Tooltip("How many seconds do we lose from the timer when we make a mistake")]
        public float timeLoss = 0;

        [Tooltip("How many seconds do we add to the timer when answering correctly")]
        public float timeBonus = 0;

        // The bonus we currently have
        internal float bonus;

        [Tooltip("The percentage we lose from our potential bonus if we answer a question wrongly. For example 0.5 makes us lose half the bonus if we answer wrongly once, and ¾ of the bonus if we answer twice incorrectly.")]
        public float bonusLoss = 0.5f;

        // The highscore recorded for a level ( used in single player only )
        internal float highScore = 0;

        [Tooltip("You can set a score limit, if you reach it you win the quiz. If you don't want a score limit just keep it at 0")]
        public float scoreToVictory = 0;

        [Tooltip("Highlight the correct answer/s when showing the result")]
        public bool showCorrectAnswer = true;

        [Tooltip("If we are using an image for a closeup game mode, we can set how close the image zooms in when displayed.")]
        public float imageCloseupZoom = 0;

        [Header("<Overwrite quiz values>")]
        [Tooltip("Set the same question followup for all questions in the quiz. This overwrites the individual followup text set on each question. It's good when you want to make a 'click to continue' button")]
        public string quizFollowup = "";

        [Tooltip("Set the same question followup for all questions in the quiz. This overwrites the individual followup text set on each question. It's good when you want to make a 'click to continue' button")]
        public Sprite quizFollowupImage;

        [Tooltip("Set the same question followup for all questions in the quiz. This overwrites the individual followup text set on each question. It's good when you want to make a 'click to continue' button")]
        public AudioClip quizFollowupSound;

        [Tooltip("Set the same question time for all questions in the quiz. This overwrites the individual times set on each question. Keep this at 0 if you don't want to overwrite any values")]
        public float quizTime = 0;

        [Tooltip("Set the same question bonus for all questions in the quiz. This overwrites the individual bonuses set on each question. Keep this at 0 if you don't want to overwrite any values")]
        public float quizBonus = 0;

        [Header("<User Interface Options>")]
        [Tooltip("The bonus object that displays how much we can win if we answer correctly")]
        public Transform bonusObject;

        [Tooltip("The menu that appears at the start of the game. This is used in the hotseat mode where we show a menu asking how many players want to participate")]
        public Transform startCanvas;

        //The canvas of the timer in the game
        internal GameObject timerIcon;
        internal Image timerBar;
        internal Text timerText;

        // This is a special animation-based timer. Instead of filling up a bar it calculates the animation of the timer
        internal Animation timerAnimated;

        [Tooltip("The menu that appears if we lose all lives in a single player game")]
        public Transform gameOverCanvas;

        [Tooltip("The menu that appears after finishing all the questions in the game. Used for single player and hotseat")]
        public Transform victoryCanvas;

        [Tooltip("The canvas that holds the larget image when we click on an image question")]
        public Transform largerImageCanvas;

        // Is the game over?
        internal bool isGameOver = false;

        [Tooltip("The level of the main menu that can be loaded after the game ends")]
        public string mainMenuLevelName = "CS_StartMenu";

        [Header("<Animation & Sounds>")]
        [Tooltip("The animation that plays when showing an answer")]
        public AnimationClip animationShow;

        [Tooltip("The animation that plays when hiding an answer")]
        public AnimationClip animationHide;

        [Tooltip("The animation that plays when choosing the correct answer")]
        public AnimationClip animationCorrect;

        [Tooltip("The animation that plays when choosing the wrong answer")]
        public AnimationClip animationWrong;

        [Tooltip("The animation that plays when showing a new question")]
        public AnimationClip animationQuestion;

        [Tooltip("Various sounds and their source")]
        public AudioClip soundQuestion;
        public AudioClip soundCorrect;
        public AudioClip soundWrong;
        public AudioClip soundTimeUp;
        public AudioClip soundGameOver;
        public AudioClip soundVictory;
        public string soundSourceTag = "Sound";
        internal GameObject soundSource;

        // This counts the time of the current sound playing now, so that we don't play another sound
        internal float soundPlayTime = 0;
        internal bool isPaused = false;

        // A general use index
        internal int index = 0;
        internal int indexB = 0;

        internal bool keyboardControls = false;
        
        // This stat keeps track of the time if took from the start of the quiz to the end of the quiz (gameover or victory)
        internal DateTime startTime;
        internal TimeSpan playTime;

        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// The difference between Awake and Start is that Start is only called if the script instance is enabled.
        /// This allows you to delay any initialization code, until it is really needed.
        /// Awake is always called before any Start functions.
        /// This allows you to order initialization of scripts
        /// </summary>
        void Start()
        {
            // Disable multitouch so that we don't tap two answers at the same time ( prevents multi-answer cheating, thanks to Miguel Paolino for catching this bug )
            Input.multiTouchEnabled = false;

            // Cache the current event system so we can enable and disable it between questions
            eventSystem = UnityEngine.EventSystems.EventSystem.current;

            // If the quiz is running on a mobile platform ( iOS, Android, etc ), disable the Standalone Input Module, so that gamepad can't take over
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                eventSystem.GetComponent<StandaloneInputModule>().enabled = false;
            }

            //Hide the game over ,victory ,and larger image screens
            if (gameOverCanvas) gameOverCanvas.gameObject.SetActive(false);
            if (victoryCanvas) victoryCanvas.gameObject.SetActive(false);
            if (largerImageCanvas) largerImageCanvas.gameObject.SetActive(false);

            //Get the highscore for the player
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			highScore = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "HighScore", 0);
#else
            highScore = PlayerPrefs.GetFloat(Application.loadedLevelName + "HighScore", 0);
#endif

            //Assign the timer icon and text for quicker access
            if (GameObject.Find("TimerIcon"))
            {
                timerIcon = GameObject.Find("TimerIcon");
                if (GameObject.Find("TimerIcon/Bar")) timerBar = GameObject.Find("TimerIcon/Bar").GetComponent<Image>();
                if (GameObject.Find("TimerIcon/Text")) timerText = GameObject.Find("TimerIcon/Text").GetComponent<Text>();
            }

            // If we have an animated timer, assign it for quicker access
            if (GameObject.Find("TimerAnimated") && GameObject.Find("TimerAnimated").GetComponent<Animation>()) timerAnimated = GameObject.Find("TimerAnimated").GetComponent<Animation>();

            // If we have a global time value, it takes over the local times of the quiz.
            if (globalTime > 0) timeLeft = globalTime;

            //Assign the players object for quicker access to player names, scores, and lives
            if (GameObject.Find("PlayersObject"))
            {
                playersObject = GameObject.Find("PlayersObject").GetComponent<RectTransform>();
            }

            // Record the default number of questions per group, so that when we change the number of players we can update this value correctly
            defaultQuestionsPerGroup = questionsPerGroup;

            // Update the current list of players based on numberOfPlayers
            SetNumberOfPlayers(numberOfPlayers);

            //Assign the sound source for easier access
            if (GameObject.FindGameObjectWithTag(soundSourceTag)) soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

            // Clear the bonus object text
            if (bonusObject) bonusObject.Find("Text").GetComponent<Text>().text = "";

            // Clear the question text
            questionObject.Find("Text").GetComponent<Text>().text = "";

            // Hide the CONTINUE button that moves to the next question
            if (questionObject.Find("ButtonContinue")) questionObject.Find("ButtonContinue").gameObject.SetActive(false);

            // Assign the image and video objects from inside the question object
            if (questionObject.Find("Image")) imageObject = questionObject.Find("Image").GetComponent<RectTransform>();
            if (questionObject.Find("Video")) videoObject = questionObject.Find("Video").gameObject;

            // Hide the sound button, since we don't need it yet
            if (questionObject.Find("ButtonPlaySound")) questionObject.Find("ButtonPlaySound").gameObject.SetActive(false);

            // Clear the question image and video, if they exist
            if (imageObject)
            {
                imageObject.gameObject.GetComponent<Image>().sprite = null;
                imageObject.gameObject.SetActive(false);
            }

            if (videoObject) videoObject.SetActive(false);
            
            // Disable the button from the question, so that we don't accidentally try to open an image that isn't there
            if (questionObject.GetComponent<Button>()) questionObject.GetComponent<Button>().enabled = false;

            // Hide multi choice confirm button, set reference, and button listener 
            if (transform.Find("MultiChoiceButton"))
            {
                multiChoiceButton = transform.Find("MultiChoiceButton").gameObject;
                multiChoiceButton.SetActive(false);
                multiChoiceButton.GetComponent<Button>().onClick.AddListener(delegate () { CheckMultiChoice(); });
            }

            // Find the "Answer" object which holds all the answer buttons, and assign all the answer buttons to an array for easier access
            if (GameObject.Find("Answers"))
            {
                // Define the answers holder which we will fill with the answer buttons
                Transform answersHolder = GameObject.Find("Answers").transform;

                // Prepare the list of answer buttons so we can fill it out
                answerObjects = new Transform[answersHolder.childCount];

                // Assign the answer buttons
                for (index = 0; index < answerObjects.Length; index++) answerObjects[index] = answersHolder.GetChild(index);

                // Listen for a click on each button to choose the answer
                foreach (Transform answerObject in answerObjects)
                {
                    // We need this temporary variable to be able to assign event listeners to multiple objects
                    Transform tempAnswerObject = answerObject;

                    // Listen for a click to choose the answer
                    tempAnswerObject.GetComponent<Button>().onClick.AddListener(delegate () { ChooseAnswer(tempAnswerObject); });
                }
            }

            // Clear all the answers
            foreach (Transform answerObject in answerObjects)
            {
                // Clear the answer text
                answerObject.Find("Text").GetComponent<Text>().text = "";

                // If the answer has an image, hide it
                if (answerObject.Find("Image")) answerObject.Find("Image").gameObject.SetActive(false);

                // If the answer has a video, hide it
                if (answerObject.Find("Video")) answerObject.Find("Video").gameObject.SetActive(false);

                // Hide answer outline 
                if (answerObject.Find("Outline")) answerObject.Find("Outline").GetComponent<Image>().enabled = false;

                // Deactivate the answer object
                answerObject.gameObject.SetActive(false);
            }

            // If we have a start canvas, pause the game and display it. Otherwise, just start the game.
            if (startCanvas)
            {
                // Show the start screen
                startCanvas.gameObject.SetActive(true);
            }
            else
            {
                // Start the game! Setup the question list ( shuffle it ) and ask the first question
                StartCoroutine(StartGame());
            }
        }

        /// <summary>
        /// Prepares the question list and starts the game. Also loads the XML questions from an online address if it exists.
        /// </summary>
        public IEnumerator StartGame()
        {
            isGameOver = false;

            // Record the game start time so we can check how long the quiz took at the end of the game
            startTime = DateTime.Now;

            // The index of the first question in the question list is actually -1, so we adjust the number from the component ( 1 becomes -1, 2 becomes 0, 10 becomes 8, etc )
            currentQuestion = firstQuestion - 2;

            // Reset the question counter
            questionCount = 0;

            // Make sure the question limit isn't larger than the actual number of questions available
            questionLimit = Mathf.Clamp(questionLimit, 0, questions.Length);

            // Assign the progress object and all related objects for easier access
            if (GameObject.Find("ProgressObject"))
            {
                progressObject = GameObject.Find("ProgressObject").GetComponent<RectTransform>();

                // If we have a tab object in the progress object, assign it and duplicate the tabs based on the total number of questions we are limited to
                if (GameObject.Find("ProgressObject/Tab"))
                {
                    // Assign the tab object
                    progressTabObject = GameObject.Find("ProgressObject/Tab");

                    for (index = 0; index < questionLimit; index++)
                    {
                        // Duplicate the tab object
                        Transform newTab = Instantiate(progressTabObject.transform) as Transform;

                        // Put it inside the progress object
                        newTab.SetParent(progressObject.transform);

                        // Reset the scale and position of the tab so it fits the progress object
                        newTab.localScale = Vector3.one;
                        newTab.localPosition = Vector3.one;

                        // Set the number of the tab in text
                        newTab.Find("Text").GetComponent<Text>().text = (index + 1).ToString();
                    }

                    // Calculate the size of a single tab. This is used to align the progress bar to the center.
                    progressTabSize = progressObject.GetComponent<GridLayoutGroup>().cellSize.x;

                    // Deactivate the original tab object
                    progressTabObject.SetActive(false);
                }

                // If we have a text object in the progress object, assign it and set the number of the first question
                if (GameObject.Find("ProgressObject/Text"))
                {
                    // Assign the text object
                    progressTextObject = GameObject.Find("ProgressObject/Text");

                    // Reset the question limit counter
                    questionLimitCount = 0;

                    // Update the question count in the text
                    if (progressTextObject) progressTextObject.GetComponent<Text>().text = questionLimitCount.ToString() + "/" + questionLimit.ToString();
                }
            }

            // Check if we have a category component attached to this gamecontroller
            if (GetComponent<Category>())
            {
                // Override the current questions list with the questions from the attached category
                questions = GetComponent<Category>().questions;

                // Get the name of the category we loaded. This is used to check if we completed the category
                currentCategory = GetComponent<Category>().categoryName;
            }
            else if (GetComponent<CategoryMath>())
            {
                // Override the current questions list with the questions from the attached math category
                questions = GetComponent<CategoryMath>().questions;

                // Get the name of the category we loaded. This is used to check if we completed the category
                currentCategory = GetComponent<CategoryMath>().categoryName;
            }

            // Wait for the next frame. This line was added because the function needs to return something or it won't work.
            yield return new WaitForFixedUpdate();

            // Check if we have a Dynamic XML component in the level
            if (dynamicXML == null) dynamicXML = (TQGDynamicXML)FindObjectOfType(typeof(TQGDynamicXML));

            // If we have a Dynamic XML component, get the quiz from it
            if (dynamicXML != null)
            {
                // Try to load the questions from the quiz
                dynamicXML.SendMessage("GetXMLFromAddress", dynamicXML.addressType.ToString());

                // Wait some time before checking if the XML was loaded
                yield return new WaitForSeconds(dynamicXML.xmlLoadTimeout);


            }

            // Go through all the questions and overwrite their followup text values with the quiz-wide values, if they exist
            if (quizFollowup != String.Empty) foreach (Question question in questions) question.followup = quizFollowup;

            // Go through all the questions and overwrite their followup image values with the quiz-wide values, if they exist
            if (quizFollowupImage != null) foreach (Question question in questions) question.followupImage = quizFollowupImage;

            // Go through all the questions and overwrite their followup sound values with the quiz-wide values, if they exist
            if (quizFollowupSound != null) foreach (Question question in questions) question.followupSound = quizFollowupSound;

            // Go through all the questions and overwrite their time values with the quiz-wide values, if they exist
            if (quizTime > 0) foreach (Question question in questions) question.time = quizTime;

            // Go through all the questions and overwrite their bonus values with the quiz-wide values, if they exist
            if (quizBonus > 0) foreach (Question question in questions) question.bonus = quizBonus;

            // Set the list of questions for this match
            SetQuestionList();

            // Ask the first question
            StartCoroutine(AskQuestion(false));
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            // Move the progress object so that the current question is centered in the screen
            if (progressObject && progressTabObject) progressObject.anchoredPosition = new Vector2(Mathf.Lerp(progressObject.anchoredPosition.x, progressTabSize * (0.5f - questionLimitCount), Time.deltaTime * 10), progressObject.anchoredPosition.y);

            if (currentPlayer < players.Length)
            {
                // Move the players object so that the current player is centered in the screen
                if (players[currentPlayer].nameText && bonusObject.position.x != players[currentPlayer].nameText.transform.position.x)
                {
                    playersObject.anchoredPosition = new Vector2(Mathf.Lerp(playersObject.anchoredPosition.x, currentPlayer * -200 - 100, Time.deltaTime * 10), playersObject.anchoredPosition.y);
                }

                // Make the score count up to its current value, for the current player
                if (players[currentPlayer].score < players[currentPlayer].scoreCount)
                {
                    // Count up to the courrent value
                    players[currentPlayer].score = Mathf.Lerp(players[currentPlayer].score, players[currentPlayer].scoreCount, Time.deltaTime * 10);

                    // Round up the score value
                    players[currentPlayer].score = Mathf.CeilToInt(players[currentPlayer].score);

                    // Update the score text
                    UpdateScore();
                }

                // Update the lives bar
                if (players[currentPlayer].livesBar)
                {
                    // If the lives bar has a text in it, update it. Otherwise, resize the lives bar based on the number of lives left
                    if (players[currentPlayer].livesBar.transform.Find("Text")) players[currentPlayer].livesBar.transform.Find("Text").GetComponent<Text>().text = players[currentPlayer].lives.ToString();
                    else players[currentPlayer].livesBar.rectTransform.sizeDelta = Vector2.Lerp(players[currentPlayer].livesBar.rectTransform.sizeDelta, new Vector2(players[currentPlayer].lives * livesBarWidth, players[currentPlayer].livesBar.rectTransform.sizeDelta.y), Time.deltaTime * 8);
                }
            }

            if (isGameOver == false)
            {
                // If we use the keyboard or gamepad, keyboardControls take effect
                if (keyboardControls == false && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
                {
                    keyboardControls = true;

                    // If no answer is selected, select the next available answer button
                    if (askingQuestion == true && eventSystem.firstSelectedGameObject == null)
                    {
                        // Go through the answer buttons and select the first available one
                        for (index = 0; index < answerObjects.Length; index++)
                        {
                            if (answerObjects[index].GetComponent<Button>().IsInteractable() == true)
                            {
                                eventSystem.SetSelectedGameObject(answerObjects[index].gameObject);

                                break;
                            }
                        }
                    }
                }

                // If we move the mouse in any direction or click it, or touch the screen on a mobile device, then keyboard/gamepad controls are lost
                if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0 || Input.GetMouseButtonDown(0) || Input.touchCount > 0) keyboardControls = false;

                // Count down the time until game over
                if (timeLeft > 0 && timerRunning == true)
                {
                    // Count down the time
                    timeLeft -= Time.deltaTime;
                }

                // Update the timer
                UpdateTime();
            }
            // Update the play sound time
            if (soundPlayTime > 0) soundPlayTime -= Time.deltaTime;
        }

        /// <summary>
        /// Sets the question list, first shuffling them, then sorting them by bonus value, 
        /// and finally choosing a limited number of questions from each bonus group
        /// </summary>
        void SetQuestionList()
        {
            // Shuffle all the available questions
            if (randomizeQuestions == true) questions = ShuffleQuestions(questions);

            // Sort the questions into groups by the bonus they give, from lowest to highest
            if (sortQuestions == true)
            {
                Array.Sort(questions, delegate (Question x, Question y) { return x.bonus.CompareTo(y.bonus); });
            }
            else
            {
                // If we are not sorting by bonus groups, then there is no need to limit the number of questions per group
                questionsPerGroup = questions.Length;
            }
        }

        /// <summary>
        /// Shuffles the specified questions list, and returns it
        /// </summary>
        /// <param name="questions">A list of questions</param>
        Question[] ShuffleQuestions(Question[] questions)
        {
            // Go through all the questions and shuffle them
            for (index = 0; index < questions.Length; index++)
            {
                // Hold the question in a temporary variable
                Question tempQuestion = questions[index];

                // Choose a random index from the question list
                int randomIndex = UnityEngine.Random.Range(index, questions.Length);

                // Assign a random question from the list
                questions[index] = questions[randomIndex];

                // Assign the temporary question to the random question we chose
                questions[randomIndex] = tempQuestion;
            }

            return questions;
        }

        /// <summary>
        /// Shuffles the specified answers list, and returns it
        /// </summary>
        /// <param name="answers">A list of answers</param>
        Answer[] ShuffleAnswers(Answer[] answers)
        {
            // Go through all the answers and shuffle them
            for (index = 0; index < answers.Length; index++)
            {
                // Hold the question in a temporary variable
                Answer tempAnswer = answers[index];

                // Choose a random index from the question list
                int randomIndex = UnityEngine.Random.Range(index, answers.Length);

                // Assign a random question from the list
                answers[index] = answers[randomIndex];

                // Assign the temporary question to the random question we chose
                answers[randomIndex] = tempAnswer;
            }

            return answers;
        }



        /// <summary>
        /// Presents a question from the list, along with possible answers.
        /// </summary>
        IEnumerator AskQuestion(bool animateQuestion)
        {
            if (isGameOver == false)
            {
                // This boolean is used to check if we already asked this question, and then ask another instead
                bool questionIsUsed = false;

                // We are now asking a question
                askingQuestion = true;

                // If we asked enough questions, move on to the next bonus group
                if (questionCount >= questionsPerGroup)
                {
                    // Holding the current bonus to compare to the next bonus
                    float tempBonus = questions[currentQuestion].bonus;

                    // Move through the questions until you find a question with a higher bonus
                    while (tempBonus >= questions[currentQuestion].bonus)
                    {
                        // Go to the next question
                        currentQuestion++;

                        // If we ran out of questions, stop checking
                        if (currentQuestion >= questions.Length) break;

                        // Check if the question has already been used, and if so, ask another question instead
                        if (dontRepeatQuestions == true && PlayerPrefs.HasKey(questions[currentQuestion].question)) questionIsUsed = true;
                    }

                    // Animate the question
                    if (animationQuestion)
                    {
                        // If the animation clip doesn't exist in the animation component, add it
                        if (questionObject.GetComponent<Animation>().GetClip(animationQuestion.name) == null) questionObject.GetComponent<Animation>().AddClip(animationQuestion, animationQuestion.name);

                        // Play the animation
                        questionObject.GetComponent<Animation>().Play(animationQuestion.name);

                        // Wait for half the animation time, then display the next question. This will make the question appear while the question tab flips. Just a nice effect
                        yield return new WaitForSeconds(questionObject.GetComponent<Animation>().clip.length * 0.5f);
                    }

                    // Reset the question counter
                    questionCount = 0;
                }
                else
                {
                    // Go to the next question
                    currentQuestion++;

                    // If we got to the last question in the quiz, check if there are unused questions we can put in the quiz again
                    if (dontRepeatQuestions == true && currentQuestion >= questions.Length)
                    {
                        // Go through all the questions and reset their "used" status, so that we can use them in the quiz again
                        for (index = 0; index < questions.Length; index++) PlayerPrefs.DeleteKey(questions[index].question);

                        // Display a text indicating that we are resetting the question list 
                        questionObject.Find("Text").GetComponent<Text>().text = " All questions in the quiz have been asked, \nResetting questions list... ";

                        // Wait a couple of seconds to display the reset message
                        yield return new WaitForSeconds(2.0f);

                        // Reset the question index back to 0, so we begin from the first question
                        currentQuestion = 0;
                    }

                    // Check if the question has already been used, and if so, ask another question instead
                    if (currentQuestion < questions.Length && dontRepeatQuestions == true && PlayerPrefs.HasKey(questions[currentQuestion].question)) questionIsUsed = true;

                    // Animate the question
                    if (animationQuestion && questionIsUsed == false)
                    {
                        // If the animation clip doesn't exist in the animation component, add it
                        if (questionObject.GetComponent<Animation>().GetClip(animationQuestion.name) == null) questionObject.GetComponent<Animation>().AddClip(animationQuestion, animationQuestion.name);

                        // Play the animation
                        questionObject.GetComponent<Animation>().Play(animationQuestion.name);

                        // Wait for half the animation time, then display the next question. This will make the question appear while the question tab flips. Just a nice effect
                        yield return new WaitForSeconds(questionObject.GetComponent<Animation>().clip.length * 0.5f);
                    }
                }

                if (questionIsUsed == false)
                {
                    // If we still have questions in the list, ask the next question
                    if (currentQuestion < questions.Length)
                    {
                        // Display the current question
                        questionObject.Find("Text").GetComponent<Text>().text = questions[currentQuestion].question;

                        // if the question is multi choice, show check button
                        if (multiChoiceButton) multiChoiceButton.SetActive(questions[currentQuestion].multiChoice);

                        // Record this question in PlayerPrefs so that we know it has been asked
                        PlayerPrefs.SetInt(questions[currentQuestion].question, 1);

                        // Set the time for this question, unless we have a global timer, in which case ignore the local time of the question
                        if (globalTime <= 0) timeLeft = questions[currentQuestion].time;

                        // Start the timer
                        timerRunning = true;

                        // If the question has a sound, activate the sound button so we can play it
                        if (questionObject.Find("ButtonPlaySound"))
                        {
                            if ( questions[currentQuestion].sound != null && questions[currentQuestion].soundURL != String.Empty)
                            {
                                StartCoroutine(LoadSoundFromURL(questions[currentQuestion].soundURL, questions[currentQuestion], null, null));
                            }
                            else if (questions[currentQuestion].sound != null )
                            {
                                // Activate the sound button, since it may have been hidden before
                                questionObject.Find("ButtonPlaySound").gameObject.SetActive(true);

                                // Play the sound when the question is displayed
                                if (questionLimitCount < questionLimit || questionLimit == 0) PlayQuestionSound();
                            }
                            else
                            {
                                // Hide the sound button, since we don't need it in this answer
                                questionObject.Find("ButtonPlaySound").gameObject.SetActive(false);
                            }
                        }

                        if ( questions[currentQuestion].image || questions[currentQuestion].imageURL != String.Empty )    // If we have a question image, display it. Otherwise, hide the image object
                        {
                            // Hide the video object
                            if (videoObject) videoObject.gameObject.SetActive(false);

                            // Unhide the image object
                            if (imageObject )
                            {
                                // If we have an image URL, try to load it
                                if ( questions[currentQuestion].imageURL != null && questions[currentQuestion].imageURL != String.Empty )
                                {
                                    StartCoroutine(LoadImageFromURL(questions[currentQuestion].imageURL, imageObject.GetComponent<Image>(), true));
                                }
                                else if (questions[currentQuestion].image) // Otherwise, take the image that is assigned locally ( from inside Unity )
                                {
                                    // Show the sprite object immediately
                                    imageObject.gameObject.SetActive(true);

                                    // Display the image for the current question
                                    imageObject.GetComponent<Image>().sprite = questions[currentQuestion].image;

                                    // Add a button to the image, so that we can enlarge it when we click it
                                    if (questionObject.GetComponent<Button>() == null) questionObject.gameObject.AddComponent<Button>();

                                    // Listen for a click on the button to enlarge the image in it
                                    questionObject.GetComponent<Button>().onClick.AddListener(delegate () { ShowLargerImage(largerImageCanvas); });

                                    // Enable the button from the question, so that we can click it to open a larger image
                                    if (questionObject.GetComponent<Button>()) questionObject.GetComponent<Button>().enabled = true;
                                }

                                // If we have a closeup of the current image, zoom in to conceal it
                                if (imageCloseupZoom > 0)
                                {
                                    imageObject.offsetMin = Vector2.one * -imageCloseupZoom;
                                    imageObject.offsetMax = Vector2.one * imageCloseupZoom;
                                }
                            }
                        }
                        else // Show the question without an image or a video
                        {
                            // Hide the image object
                            if (imageObject) imageObject.gameObject.SetActive(false);

                            // Disable the button from the question, so that we don't accidentally try to open an image that isn't there
                            if (questionObject.GetComponent<Button>()) questionObject.GetComponent<Button>().enabled = false;

                            // Hide the video object
                            if (videoObject) videoObject.gameObject.SetActive(false);
                        }

                        // Clear all the answers
                        foreach (Transform answerObject in answerObjects)
                        {
                            answerObject.Find("Text").GetComponent<Text>().text = "";

                            // Hide the answer outline
                            if (answerObject.Find("Outline"))    answerObject.Find("Outline").GetComponent<Image>().enabled = false;

                            // If the answer has an image, clear it and hide it
                            if (answerObject.Find("Image"))
                            {
                                answerObject.Find("Image").GetComponent<Image>().sprite = null;
                                answerObject.Find("Image").gameObject.SetActive(false);
                            }

                            // If the answer has a video, hide it
                            if (answerObject.Find("Video")) answerObject.Find("Video").gameObject.SetActive(false);
                        }

                        // Shuffle the list of answers
                        if (randomizeAnswers == true) questions[currentQuestion].answers = ShuffleAnswers(questions[currentQuestion].answers);

                        // Display the wrong and correct answers in the answer slots
                        for (index = 0; index < questions[currentQuestion].answers.Length; index++)//  answerObjects.Length; index++)
                        {
                            // If the answer object is inactive, activate it
                            if (answerObjects[index].gameObject.activeSelf == false) answerObjects[index].gameObject.SetActive(true);

                            // Play the animation Show
                            if (animationShow)
                            {
                                // If the animation clip doesn't exist in the animation component, add it
                                if (answerObjects[index].GetComponent<Animation>().GetClip(animationShow.name) == null) answerObjects[index].GetComponent<Animation>().AddClip(animationShow, animationShow.name);

                                // Play the animation
                                answerObjects[index].GetComponent<Animation>().Play(animationShow.name);
                            }

                            // Enable the button so we can press it
                            answerObjects[index].GetComponent<Button>().interactable = true;

                            // Select each button as it becomes enabled. This action solves a bug that appeared in Unity 5.5 where buttons stay highlighted from the previous question.
                            answerObjects[index].GetComponent<Button>().Select();

                            // Display the text of the answer
                            if (index < questions[currentQuestion].answers.Length) answerObjects[index].Find("Text").GetComponent<Text>().text = questions[currentQuestion].answers[index].answer;
                            else answerObjects[index].gameObject.SetActive(false);

                            // If the answer has a sound, activate the sound button so we can play it
                            if (answerObjects[index].Find("ButtonPlaySound"))
                            {
                                if (questions[currentQuestion].answers[index].sound != null && questions[currentQuestion].answers[index].soundURL != String.Empty)
                                {
                                    StartCoroutine(LoadSoundFromURL(questions[currentQuestion].answers[index].soundURL, null, answerObjects[index], questions[currentQuestion].answers[index]));
                                }
                                else if (questions[currentQuestion].answers[index].sound != null)
                                {
                                    // Activate the sound button, since it may have been hidden before
                                    answerObjects[index].Find("ButtonPlaySound").gameObject.SetActive(true);
                                }
                                else
                                {
                                    // Hide the sound button, since we don't need it in this answer
                                    answerObjects[index].Find("ButtonPlaySound").gameObject.SetActive(false);
                                }
                            }

                            // If the answer has an image, display it
                            if (answerObjects[index].Find("Image") && (questions[currentQuestion].answers[index].image || questions[currentQuestion].answers[index].imageURL != String.Empty))
                            {
                                // If we have an image URL, try to load it
                                if (questions[currentQuestion].answers[index].imageURL != null && questions[currentQuestion].answers[index].imageURL != String.Empty)
                                {
                                    StartCoroutine(LoadImageFromURL(questions[currentQuestion].answers[index].imageURL, answerObjects[index].Find("Image").GetComponent<Image>(), false));
                                }
                                else if (questions[currentQuestion].answers[index].image) // Otherwise, take the image that is assigned locally ( from inside Unity )
                                {
                                    // Show the sprite object immediately
                                    answerObjects[index].Find("Image").gameObject.SetActive(true);

                                    // Assign the image to the answer
                                    answerObjects[index].Find("Image").GetComponent<Image>().sprite = questions[currentQuestion].answers[index].image;
                                }
                            }
                        }

                        // If we started a new bonus group, reset the question counter
                        if (bonus > questions[currentQuestion].bonus) questionCount = 0;

                        // Set the bonus we can get for this question 
                        bonus = questions[currentQuestion].bonus;

                        if (bonusObject && bonusObject.GetComponent<Animation>())
                        {
                            // Animate the bonus object
                            bonusObject.GetComponent<Animation>().Play();

                            // Reset the bonus animation
                            bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = -1;

                            // Display the bonus text
                            bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();
                        }

                        // If keyboard controls are on, highlight the first answer. Otherwise, deselect all answers
                        if (keyboardControls == true) eventSystem.SetSelectedGameObject(answerObjects[0].gameObject);
                        else eventSystem.SetSelectedGameObject(null);

                        //If there is a source and a sound, play it from the source
                        if (soundSource && soundQuestion) soundSource.GetComponent<AudioSource>().PlayOneShot(soundQuestion);
                    }
                    else // If we have no more questions in the list, win the game
                    {
                        //Disable the question object
                        //questionObject.gameObject.SetActive(false);

                        //If we have no more questions, we win the game!
                        StartCoroutine(Victory(0));
                    }

                    // If we have a question limit, count towards it to win
                    if (isGameOver == false && questionLimit > 0)
                    {
                        questionLimitCount++;

                        if (progressTextObject)
                        {
                            // Update the question count in the text
                            progressTextObject.GetComponent<Text>().text = questionLimitCount.ToString() + "/" + questionLimit.ToString();
                        }

                        // If we reach the question limit, win the game
                        if (questionLimitCount > questionLimit) StartCoroutine(Victory(0));
                    }
                }
                else
                {
                    // Ask the next question
                    StartCoroutine(AskQuestion(true));
                }

            }
        }

        /// <summary>
        /// Chooses an answer from the list by index
        /// </summary>
        /// <param name="answerIndex">The number of the answer we chose</param>
        public void ChooseAnswer(Transform answerSource)
        {
            // Get the index of this answer object
            int answerIndex = answerSource.GetSiblingIndex();

            // We can only choose an answer if a question is being asked now
            if (askingQuestion == true)
            {
                // If this is a multi-choice question, allow the player to choose more than one question before checking the result
                if ( questions[currentQuestion].multiChoice == true )
                {
                    if (answerObjects[answerIndex].Find("Outline")) answerObjects[answerIndex].Find("Outline").GetComponent<Image>().enabled = !answerObjects[answerIndex].Find("Outline").GetComponent<Image>().enabled;

                    return;
                }

                // If the chosen answer is wrong, disable it and reduce the bonus for this question
                //if ( answerObjects[answerIndex].Find("Text").GetComponent<Text>().text != questions[currentQuestion].correctAnswer )
                if (questions[currentQuestion].answers[answerIndex].isCorrect == false)
                {
                    // Play the animation Wrong
                    if (animationWrong)
                    {
                        // If the animation clip doesn't exist in the animation component, add it
                        if (answerObjects[answerIndex].GetComponent<Animation>().GetClip(animationWrong.name) == null) answerObjects[answerIndex].GetComponent<Animation>().AddClip(animationWrong, animationWrong.name);

                        // Play the animation
                        answerObjects[answerIndex].GetComponent<Animation>().Play(animationWrong.name);
                    }

                    // Disable the button so we can't press it again
                    answerObjects[answerIndex].GetComponent<Button>().interactable = false;

                    // If no answer is selected, select the next available answer button
                    if (eventSystem.firstSelectedGameObject == null)
                    {
                        // Go through the answer buttons and select the first available one
                        for (index = 0; index < answerObjects.Length; index++)
                        {
                            if (answerObjects[index].GetComponent<Button>().IsInteractable() == true)
                            {
                                if ( Application.isMobilePlatform == false && keyboardControls == true )    eventSystem.SetSelectedGameObject(answerObjects[index].gameObject);

                                break;
                            }
                        }
                    }

                    // Set the color of the tab to red, if it exists
                    if (progressTabObject) progressObject.transform.GetChild(questionLimitCount).GetComponent<Image>().color = Color.red;

                    // Cut the bonus to half its current value
                    bonus *= bonusLoss;

                    // Lose some time as a penalty
                    timeLeft -= timeLoss;

                    // Display the bonus text
                    if (bonusObject) bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();

                    // Increase the mistake count
                    mistakeCount++;

                    // If we reach the maximum number of mistakes, give no bonus and move on to the next question
                    if (mistakeCount >= maximumMistakes)
                    {
                        // Give no bonus
                        bonus = 0;

                        // Display the bonus text
                        if (bonusObject) bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();

                        // Reduce from lives
                        players[currentPlayer].lives--;

                        // Update the lives we have left
                        Updatelives();

                        // Add to the stat wrong answer
                        wrongAnswers++;

                        // Show the result of this question, which is wrong
                        ShowResult(false);
                    }
                    else
                    {
                        // If we have more than one player and we are playing in turns (hotseat), go to the next player turn
                        if (playInTurns == true)
                        {
                            if (currentPlayer < numberOfPlayers - 1) currentPlayer++;
                            else currentPlayer = 0;
                        }
                    }

                    //If there is a source and a sound, play it from the source
                    if (soundSource && soundWrong) soundSource.GetComponent<AudioSource>().PlayOneShot(soundWrong);
                }
                else // Choosing the correct answer
                {
                    // If we answered correctly this round, increase the question count for this bonus group
                    questionCount++;

                    // Increase the count of the correct answers. This is used to show how many answers we got right at the end of the game
                    correctAnswers++;

                    // Play the animation Correct
                    if (animationCorrect)
                    {
                        // If the animation clip doesn't exist in the animation component, add it
                        if (answerObjects[answerIndex].GetComponent<Animation>().GetClip(animationCorrect.name) == null) answerObjects[answerIndex].GetComponent<Animation>().AddClip(animationCorrect, animationCorrect.name);

                        // Play the animation
                        answerObjects[answerIndex].GetComponent<Animation>().Play(animationCorrect.name);
                    }

                    // If we have a progress object, color the question tab based on the relevant answer object
                    if (progressObject)
                    {
                        // Set the color of the tab to green, if it exists
                        if (progressTabObject) progressObject.transform.GetChild(questionLimitCount).GetComponent<Image>().color = Color.green;
                    }

                    // Animate the bonus being added to the score
                    if (bonusObject && bonusObject.GetComponent<Animation>())
                    {
                        // Play the animation
                        bonusObject.GetComponent<Animation>().Play();

                        // Reset the speed of the animation
                        bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = 1;
                    }

                    // Add the bonus to the score of the current player
                    players[currentPlayer].scoreCount += bonus;

                    // Add the time bonus to the time left, if we have a global timer
                    if (globalTime > 0)
                    {
                        timeLeft += timeBonus;

                        // If we have go beyond the original global time value, updated the fill bar to accomodate the new value
                        if (timeLeft > globalTime) globalTime = timeLeft;
                    }

                    //If there is a source and a sound, play it from the source
                    if (soundSource && soundCorrect) soundSource.GetComponent<AudioSource>().PlayOneShot(soundCorrect);

                    // Show the result of this question, which is correct
                    ShowResult(true);
                }
            }
        }

        public void CheckMultiChoice()
        {
            // Hide the check button
            multiChoiceButton.SetActive(false);

            bool goodResult = true;

            for (int answerIndex = 0; answerIndex < questions[currentQuestion].answers.Length; answerIndex++)
            {
                // Disable the button so we can't press it again
                answerObjects[answerIndex].GetComponent<Button>().interactable = false;

                if ( questions[currentQuestion].answers[answerIndex].isCorrect == answerObjects[answerIndex].Find("Outline").GetComponent<Image>().enabled )
                {
                    // Play the animation Wrong
                    if (animationCorrect)
                    {
                        // If the animation clip doesn't exist in the animation component, add it
                        if (answerObjects[answerIndex].GetComponent<Animation>().GetClip(animationCorrect.name) == null) answerObjects[answerIndex].GetComponent<Animation>().AddClip(animationCorrect, animationCorrect.name);

                        // Play the animation
                        answerObjects[answerIndex].GetComponent<Animation>().Play(animationCorrect.name);
                    }
                }
                else
                {
                    // Play the animation Wrong
                    if (animationWrong)
                    {
                        // If the animation clip doesn't exist in the animation component, add it
                        if (answerObjects[answerIndex].GetComponent<Animation>().GetClip(animationWrong.name) == null) answerObjects[answerIndex].GetComponent<Animation>().AddClip(animationWrong, animationWrong.name);

                        // Play the animation
                        answerObjects[answerIndex].GetComponent<Animation>().Play(animationWrong.name);
                    }

                    goodResult = false;
                }
            }

            if (goodResult == true )
            {
                // If we answered correctly this round, increase the question count for this bonus group
                questionCount++;

                // Increase the count of the correct answers. This is used to show how many answers we got right at the end of the game
                correctAnswers++;

                // If we have a progress object, color the question tab based on the relevant answer object
                if (progressObject)
                {
                    // Set the color of the tab to green, if it exists
                    if (progressTabObject) progressObject.transform.GetChild(questionLimitCount).GetComponent<Image>().color = Color.green;
                }

                // Animate the bonus being added to the score
                if (bonusObject && bonusObject.GetComponent<Animation>())
                {
                    // Play the animation
                    bonusObject.GetComponent<Animation>().Play();

                    // Reset the speed of the animation
                    bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = 1;
                }

                // Add the bonus to the score of the current player
                players[currentPlayer].scoreCount += bonus;

                // Add the time bonus to the time left, if we have a global timer
                if (globalTime > 0)
                {
                    timeLeft += timeBonus;

                    // If we have go beyond the original global time value, updated the fill bar to accomodate the new value
                    if (timeLeft > globalTime) globalTime = timeLeft;
                }

                //If there is a source and a sound, play it from the source
                if (soundSource && soundCorrect) soundSource.GetComponent<AudioSource>().PlayOneShot(soundCorrect);
            }
            else
            {
                // If no answer is selected, select the next available answer button
                if (eventSystem.firstSelectedGameObject == null)
                {
                    // Go through the answer buttons and select the first available one
                    for (index = 0; index < answerObjects.Length; index++)
                    {
                        if (answerObjects[index].GetComponent<Button>().IsInteractable() == true)
                        {
                            if (Application.isMobilePlatform == false && keyboardControls == true) eventSystem.SetSelectedGameObject(answerObjects[index].gameObject);

                            break;
                        }
                    }
                }

                // Set the color of the tab to red, if it exists
                if (progressTabObject) progressObject.transform.GetChild(questionLimitCount).GetComponent<Image>().color = Color.red;

                // Cut the bonus to half its current value
                bonus *= bonusLoss;

                // Lose some time as a penalty
                timeLeft -= timeLoss;

                // Display the bonus text
                if (bonusObject) bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();

                // Increase the mistake count
                mistakeCount++;

                // If we reach the maximum number of mistakes, give no bonus and move on to the next question
                if (mistakeCount >= 1)
                {
                    // Give no bonus
                    bonus = 0;

                    // Display the bonus text
                    if (bonusObject) bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();

                    // Reduce from lives
                    players[currentPlayer].lives--;

                    // Update the lives we have left
                    Updatelives();

                    // Add to the stat wrong answer
                    wrongAnswers++;

                    // Show the result of this question, which is wrong
                    ShowResult(false);
                }
                else
                {
                    // If we have more than one player and we are playing in turns (hotseat), go to the next player turn
                    if (playInTurns == true)
                    {
                        if (currentPlayer < numberOfPlayers - 1) currentPlayer++;
                        else currentPlayer = 0;
                    }
                }

                //If there is a source and a sound, play it from the source
                if (soundSource && soundWrong) soundSource.GetComponent<AudioSource>().PlayOneShot(soundWrong);
            }

            ShowResult(goodResult);
        }

        /// <summary>
        /// Shows the result of the question, whether we answered correctly or not. Also displays a followup text and reveals a closeup image, if they exist
        /// </summary>
        /// <param name="isCorrectAnswer">We got the correct answer.</param>
        public void ShowResult(bool isCorrectAnswer)
        {
            // We are not asking a question now
            askingQuestion = false;

            // Stop the timer
            timerRunning = false;

            // Reset the mistake counter
            mistakeCount = 0;

            // Hide the larger image screen
            if (largerImageCanvas) largerImageCanvas.gameObject.SetActive(false);

            // Disable the button from the question, so that we don't accidentally try to open an image that isn't there
            if (questionObject.GetComponent<Button>()) questionObject.GetComponent<Button>().enabled = false;

            // If we have a closeup of the current image, zoom out to reveal it
            if (imageCloseupZoom > 0 && imageObject)
            {
                StartCoroutine(RevealImage(1));
            }

            // Go through all the answers and make them unclickable
            for (index = 0; index < answerObjects.Length; index++)
            {
                // If this is the correct answer, highlight it and delay its animation
                if (index < questions[currentQuestion].answers.Length)
                {
                    if (questions[currentQuestion].answers[index].isCorrect == true)
                    {
                        // Highlight the correct answer
                        if (showCorrectAnswer == true) eventSystem.SetSelectedGameObject(answerObjects[index].gameObject);
                        else answerObjects[index].GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        // Make all the buttons uninteractable
                        answerObjects[index].GetComponent<Button>().interactable = false;
                    }
                }
            }

            // If we have a followup to the question, display it
            if ((questions[currentQuestion].followup != null && questions[currentQuestion].followup != String.Empty) || questions[currentQuestion].followupImage || (questions[currentQuestion].followupImageURL != null && questions[currentQuestion].followupImageURL != String.Empty) || questions[currentQuestion].followupSound || (questions[currentQuestion].followupSound != null && questions[currentQuestion].followupSoundURL != String.Empty))
            {
                if (questions[currentQuestion].followup != String.Empty)
                {
                    // Display the followup to the question
                    questionObject.Find("Text").GetComponent<Text>().text = questions[currentQuestion].followup;
                }

                // If there is a followup image, display it
                if (questions[currentQuestion].followupImage || questions[currentQuestion].followupImageURL != String.Empty )
                {
                    if ( questions[currentQuestion].imageURL != null && questions[currentQuestion].followupImageURL != String.Empty )
                    {
                        StartCoroutine(LoadImageFromURL(questions[currentQuestion].followupImageURL, imageObject.GetComponent<Image>(), false));
                    }
                    else if (questions[currentQuestion].followupImage)
                    {
                        // Show the sprite object immediately
                        imageObject.gameObject.SetActive(true);

                        imageObject.GetComponent<Image>().sprite = questions[currentQuestion].followupImage;
                    }
                }
                else if (questions[currentQuestion].image == null && questions[currentQuestion].followupImageURL == String.Empty)
                {
                    imageObject.gameObject.SetActive(false);
                }

                // If the question has a followup sound, activate the sound button so we can play it
                if (questionObject.Find("ButtonPlaySound"))
                {
                    // If the question has a sound, activate the sound button so we can play it
                    if (questionObject.Find("ButtonPlaySound"))
                    {
                        if (questions[currentQuestion].followupSound != null && questions[currentQuestion].followupSoundURL != String.Empty)
                        {
                            StartCoroutine(LoadSoundFromURL(questions[currentQuestion].followupSoundURL, questions[currentQuestion], null, null));
                        }
                        else if (questions[currentQuestion].followupSound != null)
                        {
                            // Activate the sound button, since it may have been hidden before
                            questionObject.Find("ButtonPlaySound").gameObject.SetActive(true);

                            // Play the sound when the question is displayed
                            PlayQuestionSound();
                        }
                        else
                        {
                            // Hide the sound button, since we don't need it in this answer
                            questionObject.Find("ButtonPlaySound").gameObject.SetActive(false);
                        }
                    }
                }

                // Add a button component to the text object that displays the followup info
                if (questionObject.GetComponent<Button>() == null) questionObject.gameObject.AddComponent<Button>();

                // If there is a button, activate it
                if (questionObject.GetComponent<Button>()) questionObject.GetComponent<Button>().enabled = true;

                // Remove any other function listening for a click on this button ( ex: Stop listening for "ShowLargerImage()" )
                questionObject.GetComponent<Button>().onClick.RemoveAllListeners();

                // Listen for a click on the button to move to the next question
                questionObject.GetComponent<Button>().onClick.AddListener(delegate () { SkipQuestion(); });

                // Set the button as selected
                eventSystem.SetSelectedGameObject(questionObject.gameObject);
            }
            else
            {
                // Reset the question and answers in order to display the next question
                StartCoroutine(ResetQuestion(0.5f));
            }
        }

        /// <summary>
        /// Resets the question and answers, in preparation for the next question
        /// </summary>
        /// <returns>The question.</returns>
        /// <param name="delay">Delay in seconds before showing the next question</param>
        IEnumerator ResetQuestion(float delay)
        {
            // Go through all the answers hide the wrong ones
            for (index = 0; index < answerObjects.Length; index++)
            {
                // If this is a wrong answer, hide it. Also if we are not supposed to show the correct answer, hide all the answers
                if (index < questions[currentQuestion].answers.Length && (questions[currentQuestion].answers[index].isCorrect == false || showCorrectAnswer == false))
                {
                    // Play the animation Hide, after the current animation is over
                    if (animationHide)
                    {
                        // If the animation clip doesn't exist in the animation component, add it
                        if (answerObjects[index].GetComponent<Animation>().GetClip(animationHide.name) == null) answerObjects[index].GetComponent<Animation>().AddClip(animationHide, animationHide.name);

                        // Play the animation queded in line after te current animation
                        answerObjects[index].GetComponent<Animation>().PlayQueued(animationHide.name);
                    }
                }
            }

            // If we are supposed to show the correct answer, find it and keep it animated longer than the other answers
            if (showCorrectAnswer == true && questions[currentQuestion].multiChoice == false)
            {
                // Go through all the answers again and highlight the correct one
                for (index = 0; index < answerObjects.Length; index++)
                {
                    // If this is the correct answer, highlight it and delay its animation
                    //if ( answerObjects[index].Find("Text").GetComponent<Text>().text == questions[currentQuestion].correctAnswer )
                    if (index < questions[currentQuestion].answers.Length && questions[currentQuestion].answers[index].isCorrect == true)
                    {
                        // Highlight the correct answer
                        eventSystem.SetSelectedGameObject(answerObjects[index].gameObject);

                        // Wait for a while
                        yield return new WaitForSeconds(0.5f);

                        // Play the animation Hide
                        if (animationHide)
                        {
                            // If the animation clip doesn't exist in the animation component, add it
                            if (answerObjects[index].GetComponent<Animation>().GetClip(animationHide.name) == null) answerObjects[index].GetComponent<Animation>().AddClip(animationHide, animationHide.name);

                            // Play the animation
                            answerObjects[index].GetComponent<Animation>().Play(animationHide.name);
                        }
                    }
                }
            }

            // For for a while or until the currently playing sound ends
            //if (soundPlayTime > 0 ) yield return new WaitForSeconds(soundPlayTime);
            //else yield return new WaitForSeconds(delay);

            yield return new WaitForSeconds(delay);

            // Stop any sounds playing, and reset the sound play time
            if (soundSource)
            {
                if ( isGameOver == false ) soundSource.GetComponent<AudioSource>().Stop();

                soundPlayTime = 0;
            }

            // If we have more than one player and we are playing in turns (hotseat), go to the next player turn
            if (numberOfPlayers > 0 && playInTurns == true)
            {
                if (currentPlayer < numberOfPlayers - 1) currentPlayer++;
                else currentPlayer = 0;
            }

            // Clear and hide the image object
            if (imageObject)
            {
                imageObject.gameObject.GetComponent<Image>().sprite = null;
                imageObject.gameObject.SetActive(false);
            }

            // Deselect the currently selected answer
            eventSystem.SetSelectedGameObject(null);

            // Ask the next question
            StartCoroutine(AskQuestion(true));
        }

        /// <summary>
        /// Updates the timer text, and checks if time is up
        /// </summary>
        void UpdateTime()
        {
            // Update the time only if we have a timer object assigned
            if (timerIcon || timerAnimated)
            {
                // Using the timer icon, which uses FillAmount and Text to show the timer we have left
                if (timerIcon)
                {
                    // Update the timer circle, if we have one
                    if (timerBar)
                    {
                        // If we have a global time, display the timer progress for it.
                        if (globalTime > 0)
                        {
                            timerBar.fillAmount = timeLeft / globalTime;
                        }
                        else if (timerRunning == true) // If the timer is running, display the fill amount left for the question time. 
                        {
                            timerBar.fillAmount = timeLeft / questions[currentQuestion].time;
                        }
                        else // Otherwise refill the amount back to 100%
                        {
                            timerBar.fillAmount = Mathf.Lerp(timerBar.fillAmount, 1, Time.deltaTime * 10);
                        }
                    }

                    // Update the timer text, if we have one
                    if (timerText)
                    {
                        // If the timer is running, display the timer left. Otherwise hide the text
                        if (timerRunning == true || globalTime > 0) timerText.text = Mathf.RoundToInt(timeLeft).ToString();
                        else timerText.text = "";
                    }
                }

                // Using the animated timer, which progresses the animation based on the timer we have left
                if (timerAnimated && timerAnimated.isPlaying == false)
                {
                    // Start the timer animation
                    timerAnimated.Play("TimerAnimatedProgress");

                    // If we have a global time, display the correct frame from the time animation
                    if (globalTime > 0)
                    {
                        timerAnimated["TimerAnimatedProgress"].time = (1 - (timeLeft / globalTime)) * timerAnimated["TimerAnimatedProgress"].clip.length;
                    }
                    else if (timerRunning == true) // If the timer is running, display the correct frame from the time animation
                    {
                        timerAnimated["TimerAnimatedProgress"].time = (1 - (timeLeft / questions[currentQuestion].time)) * timerAnimated["TimerAnimatedProgress"].clip.length;
                    }
                    else // Otherwise rewind the time animation to the start
                    {
                        timerAnimated["TimerAnimatedProgress"].time = Mathf.Lerp(timerAnimated["TimerAnimatedProgress"].time, 1, Time.deltaTime * 10);
                    }

                    // Start animating
                    timerAnimated["TimerAnimatedProgress"].enabled = true;

                    // Record the current frame
                    timerAnimated.Sample();

                    // Stop animating
                    timerAnimated["TimerAnimatedProgress"].enabled = false;
                }

                // Time's up!
                if (timeLeft <= 0 && timerRunning == true)
                {
                    // If we have a global time and the timer ran out, just go straight to the GameOver screen
                    if (globalTime > 0)
                    {
                        StartCoroutine(GameOver(1));
                    }
                    else
                    {
                        // Reduce from lives
                        players[currentPlayer].lives--;

                        // Update the lives we have left
                        Updatelives();

                        // THIS WAS REMOVED BECAUSE IT CAUSED THE GAME TO SKIP A PLAYER WHEN THE TIMER RUNS OUT IN HOTSEAT MODE
                        // If we have more than one player and we are playing in turns (hotseat), go to the next player turn
                        //if ( playInTurns == true )
                        //{
                        //if (currentPlayer < numberOfPlayers - 1) currentPlayer++;
                        //else currentPlayer = 0;
                        //}

                        // Show the result of this question, which is wrong ( because we ran out of time, we lost the question )
                        ShowResult(false);
                    }

                    // Play the timer icon animation
                    if (timerIcon && timerIcon.GetComponent<Animation>()) timerIcon.GetComponent<Animation>().Play();

                    // Play the animated timer's timeUp animation
                    if (timerAnimated && timerAnimated.GetComponent<Animation>())
                    {
                        timerAnimated.Stop();
                        timerAnimated.Play("TimerAnimatedTimeUp");
                    }

                    //If there is a source and a sound, play it from the source
                    if (soundSource && soundTimeUp) soundSource.GetComponent<AudioSource>().PlayOneShot(soundTimeUp);
                }
            }
        }

        /// <summary>
        /// Updates the score value and checks if we got to the next level
        /// </summary>
        void UpdateScore()
        {
            //Update the score text
            //if ( scoreText )    scoreText.GetComponent<Text>().text = score.ToString();

            //Update the score text for the current player
            if (players[currentPlayer].scoreText) players[currentPlayer].scoreText.GetComponent<Text>().text = players[currentPlayer].score.ToString();

            // If we reach the victory score we win the game
            if (scoreToVictory > 0 && players[currentPlayer].score >= scoreToVictory) StartCoroutine(Victory(0));
        }

        /// <summary>
        /// Runs the game over event and shows the game over screen
        /// </summary>
        IEnumerator GameOver(float delay)
        {
            isGameOver = true;

            // Calculate the quiz duration
            playTime = DateTime.Now - startTime;

            yield return new WaitForSeconds(delay);

            //Show the game over screen
            if (gameOverCanvas)
            {
                //Show the game over screen
                gameOverCanvas.gameObject.SetActive(true);

                //Write the score text, if it exists
                if (gameOverCanvas.Find("ScoreTexts/TextScore")) gameOverCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text += " " + players[currentPlayer].score.ToString();

                //Check if we got a high score
                if (players[currentPlayer].score > highScore)
                {
                    highScore = players[currentPlayer].score;

                    //Register the new high score
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
					PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", players[currentPlayer].score);
#else
                    PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", players[currentPlayer].score);
#endif
                }

                //Write the high sscore text
                gameOverCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text += " " + highScore.ToString();

                //If there is a source and a sound, play it from the source
                if (soundSource && soundGameOver) soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);
            }
        }

        /// <summary>
        /// Runs the victory event and shows the victory screen
        /// </summary>
        IEnumerator Victory(float delay)
        {
            // If this quiz has no questions at all, just return to the main menu
            if (questions.Length <= 0) yield break;

            isGameOver = true;

            // Calculate the quiz duration
            playTime = DateTime.Now - startTime;

            // Record the state of the category as completed
            if (currentCategory != null)
            {
                PlayerPrefs.SetInt(currentCategory + "Completed", 1);

                currentCategory = null;
            }
            yield return new WaitForSeconds(delay);

            //Show the game over screen
            if (victoryCanvas)
            {
                //Show the victory screen
                victoryCanvas.gameObject.SetActive(true);

                // If we have a TextScore and TextHighScore objects, then we are using the single player victory canvas
                if (victoryCanvas.Find("ScoreTexts/TextScore") && victoryCanvas.Find("ScoreTexts/TextHighScore"))
                {
                    //Write the score text, if it exists
                    victoryCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text += " " + players[currentPlayer].score.ToString();

                    //Check if we got a high score
                    if (players[currentPlayer].score > highScore)
                    {
                        highScore = players[currentPlayer].score;

                        //Register the new high score
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
						PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", players[currentPlayer].score);
#else
                        PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", players[currentPlayer].score);
#endif
                    }

                    //Write the high sscore text
                    victoryCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text += " " + highScore.ToString();
                }

                // If we have a Players object, then we are using the hotseat results canvas
                if (victoryCanvas.Find("ScoreTexts/Players"))
                {
                    // Sort the players by their score and then check the winners ( could be a draw with more than one winner )
                    // The number of winners, could be more than one in case of a draw
                    int winnerCount = 0;

                    // Sort the players by the score they have, from highest to lowest
                    Array.Sort(players, delegate (Player x, Player y) { return y.score.CompareTo(x.score); });

                    // Go through all the players and check if we have more than one winner
                    for (index = 0; index < numberOfPlayers; index++)
                    {
                        // The first player in the list is always the winner. After that we check if there are other players with the same score ( a draw between several winners )
                        if (index == 0) winnerCount = 1;
                        else if (players[index].score == players[0].score)
                        {
                            winnerCount++;
                        }
                    }

                    // Go through all the players in the table and hide the winner icon from all the losers, or if everyone got 0 points.
                    for (index = 0; index < numberOfPlayers; index++)
                    {
                        if (players[index].score <= 0 || index >= winnerCount) victoryCanvas.Find("ScoreTexts/Players").GetChild(index).Find("WinnerIcon").gameObject.SetActive(false);
                    }

                    // Go through all the score texts and update them each player
                    for (index = 0; index < numberOfPlayers; index++)
                    {
                        // Display the name of the player
                        victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetComponent<Text>().text = players[index].name;

                        // Set the color of the player name
                        victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetComponent<Text>().color = players[index].color;

                        // Display the score of the player
                        victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetChild(0).GetComponent<Text>().text = players[index].score.ToString();
                    }

                    // If the value of numberOfPlayers is lower than the actual number of players, remove any excess players from the list
                    if (numberOfPlayers < players.Length)
                    {
                        // Go through all the extra players in the list, and remove their name, score, and lives objects
                        for (index = numberOfPlayers; index < victoryCanvas.Find("ScoreTexts/Players").childCount; index++)
                        {
                            victoryCanvas.Find("ScoreTexts/Players").GetChild(index).gameObject.SetActive(false);
                        }
                    }

                    // Display the list of winners
                    // If we have one winner display a single name. Otherwise, display a "draw" message with the names of all the winners
                    if (winnerCount == 1)
                    {
                        // Display the single winner
                        victoryCanvas.Find("TextResult").GetComponent<Text>().text = players[0].name + " wins with " + players[0].score.ToString() + " points!";
                    }
                    else
                    {
                        // Display the "draw" message between several winners
                        victoryCanvas.Find("TextResult").GetComponent<Text>().text = "It's a draw between ";

                        // Display the names of the winners
                        while (winnerCount > 0)
                        {
                            winnerCount--;

                            // Add to the text the name of the next winner
                            if (winnerCount == 0) victoryCanvas.Find("TextResult").GetComponent<Text>().text += "and " + players[winnerCount].name;
                            else victoryCanvas.Find("TextResult").GetComponent<Text>().text += players[winnerCount].name + ", ";
                        }

                        // Display the score they got
                        victoryCanvas.Find("TextResult").GetComponent<Text>().text += ", each with " + players[0].score.ToString() + " points!";
                    }
                }

                // If we have a TextProgress object, then we can display how many questions we answered correctly
                if (victoryCanvas.Find("ScoreTexts/TextProgress"))
                {
                    //Write the progress text
                    victoryCanvas.Find("ScoreTexts/TextProgress").GetComponent<Text>().text = correctAnswers.ToString() + "/" + questionLimit.ToString();
                }

                //If there is a source and a sound, play it from the source
                if (soundSource && soundVictory) soundSource.GetComponent<AudioSource>().PlayOneShot(soundVictory);
            }
        }

        /// <summary>
        /// Restart the current level
        /// </summary>
        public void Restart()
        {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
#else
            Application.LoadLevel(Application.loadedLevelName);
#endif
        }

        /// <summary>
        /// Restart the current level
        /// </summary>
        public void MainMenu()
        {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(mainMenuLevelName);
#else
            Application.LoadLevel(mainMenuLevelName);
#endif
        }

        /// <summary>
        /// Updates the lives we have
        /// </summary>
        public void Updatelives()
        {
            // Update lives only if we have a lives bar assigned
            if (players[currentPlayer].livesBar)
            {
                // If we run out of lives, it's game over
                if (players[currentPlayer].lives <= 0) StartCoroutine(GameOver(1));
            }
        }

        /// <summary>
        /// Shows the larger image from the thumbnail in the question image
        /// </summary>
        public void ShowLargerImage(Transform largerImageCanvas)
        {
            if (largerImageCanvas)
            {
                // Unhide the larger image canvas
                largerImageCanvas.gameObject.SetActive(true);

                if (questions[currentQuestion].image || questions[currentQuestion].imageURL != String.Empty )
                {
                    // Hide the video object
                    largerImageCanvas.Find("Video").gameObject.SetActive(false);

                    // Unhide the image object
                    largerImageCanvas.Find("Image").gameObject.SetActive(true);

                    // Assign the image from the question to the image in the larger image canvas
                    largerImageCanvas.Find("Image").GetComponent<Image>().sprite = imageObject.GetComponent<Image>().sprite;// questions[currentQuestion].image;

                }
            }
        }

        /// <summary>
        /// Sets the number of players in the game
        /// </summary>
        /// <param name="setValue">Set value.</param>
        public void SetNumberOfPlayers(float setValue)
        {
            numberOfPlayers = Mathf.RoundToInt(setValue);

            // Multiply the number of questions per group by the number of players. 
            // This way, each player will get a chance to answer a question from the same group before moving on to the next group.
            questionsPerGroup = numberOfPlayers * defaultQuestionsPerGroup;

            // Update the text in the NumberOfPlayers button
            if (startCanvas && startCanvas.Find("NumberOfPlayers/HandleSlideArea/Handle/Text")) startCanvas.Find("NumberOfPlayers/HandleSlideArea/Handle/Text").GetComponent<Text>().text = numberOfPlayers.ToString() + " PLAYERS";

            // Create the list of players based on the number of players
            //players = new Player[numberOfPlayers];

            // Update the current list of players based on numberOfPlayers
            UpdatePlayers();
        }

        /// <summary>
        /// Update the list of players based on the number of players in the game
        /// </summary>
        void UpdatePlayers()
        {
            // Limit the value of numberOfPlayers to the actual number of players in the list
            if (numberOfPlayers > players.Length) numberOfPlayers = players.Length;

            // Go through all the extra players in the list, and remove their name, score, and lives objects
            for (index = 0; index < players.Length; index++)
            {
                if (playersObject)
                {
                    if (index < numberOfPlayers)
                    {
                        // Set the score of the active player to 0, so that it gets counted in the results page
                        players[index].score = 0;

                        // Activate the player object along with its name and score text
                        playersObject.transform.GetChild(index).gameObject.SetActive(true);
                    }
                    else
                    {
                        // Set the score of the inactive player to -1, so that it doesn't get counted in the results page
                        players[index].score = -1;

                        // Deactivate the player object along with its name and score text
                        playersObject.transform.GetChild(index).gameObject.SetActive(false);
                    }
                }
            }

            // Go through all players in the match and update the score and lives and names, if they exist
            if (players.Length > 0)
            {
                for (index = numberOfPlayers - 1; index >= 0; index--)
                {
                    // Set the current player
                    currentPlayer = index;

                    //Update the score
                    UpdateScore();

                    // Set the lives we have
                    players[currentPlayer].lives = lives;

                    // Update the lives
                    Updatelives();

                    // Set the name of each player at the start of the game
                    if (players[currentPlayer].nameText) players[currentPlayer].nameText.GetComponent<Text>().text = players[currentPlayer].name.ToString();

                    // Set the color of the player name
                    if (players[currentPlayer].nameText) players[currentPlayer].nameText.GetComponent<Text>().color = players[currentPlayer].color;
                }

                // Calculate the width of a single life in the lives bar
                if (players[currentPlayer].livesBar) livesBarWidth = players[currentPlayer].livesBar.rectTransform.sizeDelta.x / players[currentPlayer].lives;
            }
            else
            {
                Debug.LogError("You cannot play the game without setting up at least one player in the Player Options list");
            }
        }

        /// <summary>
        /// Loads an Xml file from a path, then parses it and assigns it to the selected game controller
        /// </summary>
        /// <param name="xmlPath">The path of the Xml file</param>
        public void LoadXml(string xmlPath, bool append)
        {
            // Make sure we don't reassign the answer objects when the game runs, because it will overwrite the objects with empty ones
            if (Application.isPlaying == false)
            {
                // Find the "Answer" object which holds all the answer buttons, and assign all the answer buttons to an array for easier access
                if (GameObject.Find("Answers"))
                {
                    // Define the answers holder which we will fill with the answer buttons
                    Transform answersHolder = GameObject.Find("Answers").transform;

                    // Prepare the list of answer buttons so we can fill it out
                    answerObjects = new Transform[answersHolder.childCount];
                }
            }

            /// This holds the highest number of answers for a question. It's used to check if there are extra answers that can't be shown because there are not enough Answer Objects in the game.
            /// ex: One of the questions has 5 answers, but the game only has 4 answer boxes. This will trigger a warning to tell you to increase the number of answer objects to 5 in order to hold them. 
            int extraAnswers = 0;

            // Create a new Xml document
            XmlDocument xmlDocument = new XmlDocument();

            // Load the Xml file from the path into the document
            xmlDocument.LoadXml(xmlPath);

            // Get the records from the Xml file. Each record contains a question, several answers, and bonus and time info
            xmlRecords = xmlDocument.GetElementsByTagName("record");

            int tempIndex;

            // Set the length of the question list ( in the game controller ) based on the number of questions in the Xml file
            if (append == true)
            {
                questionsTemp = new Question[questions.Length + xmlRecords.Count];
                tempIndex = questions.Length;
            }
            else
            {
                questionsTemp = new Question[xmlRecords.Count];
                tempIndex = 0;
            }

            // Go through all the questions and declare each one, so that we can fill it with info from the Xml
            for (index = tempIndex; index < questionsTemp.Length; index++) questionsTemp[index] = new Question();

            // Set the index of the question, starting from 0, which is the first question
            int questionIndex = tempIndex;

            // Go through all the Xml nodes, these are the ones that contain a Question, Image, Answers, Bonus, and Item nodes.
            foreach (XmlNode XmlRecord in xmlRecords)
            {
                // Get all the questions from the Xml record
                XmlNodeList XmlQuestions = XmlRecord.ChildNodes;

                // Go through all the Xml questions and check which part we are accessing ( Question, Image, Answers, Bonus, or Time )
                foreach (XmlNode XmlQuestion in XmlQuestions)
                {
                    // Assign the question to the right slot in the game controller
                    if (XmlQuestion.Name == "Question") questionsTemp[questionIndex].question = XmlQuestion.InnerText;
                    
                    // Assign the followup text, image, and sound slots in the game controller
                    if (XmlQuestion.Name == "Followup")
                    {
                        // Assign the followup text of the question to the correct slot in the game controller
                        questionsTemp[questionIndex].followup = XmlQuestion.InnerText;

                        // Assign the image to the right slot in the game controller. All sounds should be placed in the Resources/Sounds/ path. You should enter the name of the sound without the path and without an extension (ex; .png )
                        if (XmlQuestion.Attributes.GetNamedItem("image") != null)
                        {
                            // If it's a URL, assign it to followupImageURL
                            if ( XmlQuestion.Attributes.GetNamedItem("image").InnerText.Contains("http") )
                            {
                                questionsTemp[questionIndex].followupImageURL = XmlQuestion.Attributes.GetNamedItem("image").InnerText;
                            }
                            else // Otherwise, assign it as a sprite to the followImage
                            {
                                questionsTemp[questionIndex].followupImage = Resources.Load<Sprite>("Images/" + XmlQuestion.Attributes.GetNamedItem("image").InnerText);
                            }
                        }

                        // Assign the sound to the right slot in the game controller. All sounds should be placed in the Resources/Sounds/ path. You should enter the name of the sound without the path and without an extension (ex; .mp3 )
                        if (XmlQuestion.Attributes.GetNamedItem("sound") != null)
                        {
                            // If it's a URL, assign it to followupSoundURL
                            if (XmlQuestion.Attributes.GetNamedItem("sound").InnerText.Contains("http"))
                            {
                                questionsTemp[questionIndex].followupSoundURL = XmlQuestion.Attributes.GetNamedItem("sound").InnerText;
                            }
                            else // Otherwise, assign it as an audioclip to the followImage
                            {
                                questionsTemp[questionIndex].followupSound = Resources.Load<AudioClip>("Sounds/" + XmlQuestion.Attributes.GetNamedItem("sound").InnerText);
                            }
                        }
                    }

                    // Assign the image to the right slot in the game controller. All images should be placed in the Resources/Images/ path. You should enter the name of the image without the path and without an extension (ex; .png )
                    if (XmlQuestion.Name == "Image")
                    {
                        // If it's a URL, assign it to imageURL
                        if ( XmlQuestion.InnerText.Contains("http") )
                        {
                            questionsTemp[questionIndex].imageURL = XmlQuestion.InnerText;
                        }
                        else // Otherwise, assign it as a sprite to the image
                        {
                            questionsTemp[questionIndex].image = Resources.Load<Sprite>("Images/" + XmlQuestion.InnerText);
                        }
                    }

                    // Assign the sound to the right slot in the game controller. All sounds should be placed in the Resources/Sounds/ path. You should enter the name of the sound without the path and without an extension (ex; .mp3 )
                    if (XmlQuestion.Name == "Sound")
                    {
                        // If it's a URL, assign it to soundURL
                        if ( XmlQuestion.InnerText.Contains("http") )
                        {
                            questionsTemp[questionIndex].soundURL = XmlQuestion.InnerText;
                        }
                        else // Otherwise, assign it as an audioclip to the sound
                        {
                            questionsTemp[questionIndex].sound = Resources.Load<AudioClip>("Sounds/" + XmlQuestion.InnerText);
                        }
                    }

                    // Assign the bonus value to the right slot in the game controller
                    if (XmlQuestion.Name == "Bonus") questionsTemp[questionIndex].bonus = int.Parse(XmlQuestion.InnerText);

                    // Assign the time value to the right slot in the game controller
                    if (XmlQuestion.Name == "Time") questionsTemp[questionIndex].time = int.Parse(XmlQuestion.InnerText);

                    // Set the index of the answer, starting from 0, which is the first answer
                    int answerIndex = 0;

                    // When we detect an "Answers" record, we must go through it to find all the answers
                    if (XmlQuestion.Name == "Answers")
                    {
                        // Set the length of the answers list ( in the game controller ) based on the number of answers in the Xml file
                        questionsTemp[questionIndex].answers = new Answer[XmlQuestion.ChildNodes.Count];

                        // Go through all the answers and declare each one, so that we can fill it with info from the Xml
                        for (index = 0; index < questionsTemp[questionIndex].answers.Length; index++) questionsTemp[questionIndex].answers[index] = new Answer();

                        // Go through all the Xml nodes, these are the ones that contain an Answer, Image, and IsCorrect state nodes.
                        foreach (XmlNode XmlAnswer in XmlQuestion.ChildNodes)
                        {
                            // If this is an answer, assign it to the game controller, and also assign the IsCorrect and Image attribute, if it exists
                            if (XmlAnswer.Name.Contains("Answer"))
                            {
                                // Assign the answer
                                questionsTemp[questionIndex].answers[answerIndex].answer = XmlAnswer.InnerText;

                                // Assign the IsCorrect attribute, true or false
                                if (XmlAnswer.Attributes.GetNamedItem("correct") != null) questionsTemp[questionIndex].answers[answerIndex].isCorrect = bool.Parse(XmlAnswer.Attributes.GetNamedItem("correct").InnerText);

                                // Assign the image to the right slot in the answer. All images should be placed in the Resources/Images/ path. You should enter the name of the image without the path and without an extension (ex; .png )
                                if (XmlAnswer.Attributes.GetNamedItem("image") != null)
                                {
                                    // If it's a URL, assign it to imageURL
                                    if (XmlAnswer.Attributes.GetNamedItem("image").InnerText.Contains("http"))
                                    {
                                        questionsTemp[questionIndex].answers[answerIndex].imageURL = XmlAnswer.Attributes.GetNamedItem("image").InnerText;

                                    }
                                    else // Otherwise, assign it as a sprite to the image
                                    {
                                        questionsTemp[questionIndex].answers[answerIndex].image = Resources.Load<Sprite>("Images/" + XmlAnswer.Attributes.GetNamedItem("image").InnerText);
                                    }
                                }

                                // Assign the sound to the right slot in the game controller. All sounds should be placed in the Resources/Sounds/ path. You should enter the name of the sound without the path and without an extension (ex; .mp3 )
                                if (XmlAnswer.Attributes.GetNamedItem("sound") != null)
                                {
                                    // If it's a URL, assign it to imageURL
                                    if (XmlAnswer.Attributes.GetNamedItem("image").InnerText.Contains("http"))
                                    {
                                        questionsTemp[questionIndex].answers[answerIndex].soundURL = XmlAnswer.Attributes.GetNamedItem("sound").InnerText;
                                    }
                                    else // Otherwise, assign it as a sprite to the image
                                    {
                                        questionsTemp[questionIndex].answers[answerIndex].sound = Resources.Load<AudioClip>("Sounds/" + XmlAnswer.Attributes.GetNamedItem("sound").InnerText);
                                    }
                                }

                                if (answerIndex >= answerObjects.Length && answerIndex > extraAnswers) extraAnswers = answerIndex;
                            }

                            //if ( XmlAnswer.Name == "Image" )    questions[questionIndex].answers[answerIndex].image = Resources.Load<Sprite>("Images/" + XmlQuestion.InnerText);

                            answerIndex++;
                        }
                    }
                }

                questionIndex++;

            }

            // If we have question with extra answers, display a warning
            if (extraAnswers > 0) Debug.LogWarning("Some of the questions in the Xml file have more answers than the maximum number of Answer Objects you have in the game. Increase the number of Answer Objects to " + (extraAnswers + 1).ToString() + " to accomodate the extra answers. Read more about it in the documentation file.");

            // If we are appending questions to an existing quiz...
            if (append == true)
            {
                //...go through all the questions and copy them over to the temporary merged quiz
                for (index = 0; index < questions.Length; index++)
                {
                    questionsTemp[index] = questions[index];
                }
            }

            // Copy the temporary quiz into the final quiz
            questions = questionsTemp;
        }

        /// <summary>
        /// Writes the quiz content to and saves it as an XML file to a path
        /// </summary>
        /// <param name="xmlPath">The path of the Xml file</param>
        public string SaveXml()
        {
            // Create an empty text object which we will fill with the formatted XML
            string xmlString = "";

            // Starting the XML document with the header and data set definitions
            xmlString = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + "\n";
            xmlString += @"<data-set xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">" + "\n";

            // Go through all the questions and assign each parts to the correct XML line
            for (index = 0; index < questions.Length; index++)
            {
                // The start of a question
                xmlString += "  <record>" + "\n";

                // The question text
                xmlString += "    <Question>" + questions[index].question + "</Question>" + "\n";

                // The question image, if it exists
                if (questions[index].imageURL != String.Empty) xmlString += "    <Image>" + questions[index].imageURL + "</Image>" + "\n";
                else if (questions[index].image) xmlString += "    <Image>" + questions[index].image.name + "</Image>" + "\n";

                // The question sound, if it exists
                if (questions[index].soundURL != String.Empty) xmlString += "    <Sound>" + questions[index].soundURL + "</Sound>" + "\n";
                else if (questions[index].sound) xmlString += "    <Sound>" + questions[index].sound.name + "</Sound>" + "\n";

                // The start of the list of answers
                xmlString += "    <Answers>" + "\n";

                // Go through all the answers and declare each one, so that we can fill it with info from the Xml
                for (indexB = 0; indexB < questions[index].answers.Length; indexB++)
                {
                    // The first part of the answer element
                    xmlString += "      <Answer";

                    // The number of the answer. This allows us to load the XML into Excel and create a format for exporting from Excel to XML
                    xmlString += (indexB + 1).ToString();

                    // The answer image, if it exists
                    if (questions[index].answers[indexB].image) xmlString += " image=" + "\"" + questions[index].answers[indexB].image.name + "\"";
                    else if (questions[index].answers[indexB].image) xmlString += " image=" + "\"" + questions[index].answers[indexB].image.name + "\"";

                    // The answer sound, if it exists
                    if (questions[index].answers[indexB].soundURL != String.Empty) xmlString += " sound=" + "\"" + questions[index].answers[indexB].soundURL + "\"";
                    else if (questions[index].answers[indexB].sound) xmlString += " sound=" + "\"" + questions[index].answers[indexB].sound.name + "\"";

                    // Set the answer to be either true or false
                    if (questions[index].answers[indexB].isCorrect == true) xmlString += @" correct=""true""";
                    else xmlString += @" correct=""false""";

                    // The end of the answer element
                    xmlString += ">" + questions[index].answers[indexB].answer + "</Answer" + (indexB + 1).ToString() + ">" + "\n";
                }

                // The end of the list of answers
                xmlString += @"    </Answers>" + "\n";

                // The start of the followup element that appears after answering this question
                xmlString += @"    <Followup";

                // The answer image, if it exists
                if (questions[index].followupImageURL != String.Empty) xmlString += " image=" + "\"" + questions[index].followupImageURL + "\"";
                else if (questions[index].followupImage) xmlString += " image=" + "\"" + questions[index].followupImage.name + "\"";

                // The answer sound, if it exists
                if (questions[index].followupSoundURL != String.Empty) xmlString += " sound=" + "\"" + questions[index].followupSoundURL + "\"";
                else if (questions[index].followupSound) xmlString += " sound=" + "\"" + questions[index].followupSound.name + "\"";

                // The followup text and the end of the followup element
                xmlString += ">" + questions[index].followup + "</Followup>" + "\n";

                // The followup text that appears after answering this question
                //xmlString += @"    <Followup>" + questions[index].followup + "</Followup>" + "\n";

                // The bonus given for this question
                xmlString += @"    <Bonus>" + questions[index].bonus.ToString() + "</Bonus>" + "\n";

                // The time allowed to answer this question
                xmlString += @"    <Time>" + questions[index].time.ToString() + "</Time>" + "\n";

                // The end of a question
                xmlString += @"  </record>" + "\n";
            }

            // Ending the XML document
            xmlString += @"</data-set>" + "\n";

            return xmlString;
        }





        /// <summary>
        /// Sets the questions list from an external question list. This is used when getting the questions from a category selector.
        /// </summary>
        /// <param name="setValue">The list of questions we got</param>
        public void SetQuestions(Question[] setValue)
        {
            questions = setValue;
        }

        /// <summary>
        /// Sets the name of the category from an external category grid. This is used when getting the category name from a category grid
        /// </summary>
        /// <param name="setValue">Set value.</param>
        public void SetCategoryName(string setValue)
        {
            currentCategory = setValue;
        }

        /// <summary>
        /// Skips the current question and displays the next one
        /// </summary>
        public void SkipQuestion()
        {
            // Stop listening for a click on the button to move to the next question
            if (questionObject.GetComponent<Button>()) questionObject.GetComponent<Button>().onClick.RemoveAllListeners();

            // Reset the question and answers in order to display the next question
            StartCoroutine(ResetQuestion(0.5f));
        }

        /// <summary>
        /// Reveals a closeup image by zooming out to show it
        /// </summary>
        /// <param name="delay">How many seconds it takes to fully reveal the image</param>
        IEnumerator RevealImage(float revealTime)
        {
            // If we have a closeup of the current image, zoom out to reveal it
            if (imageCloseupZoom > 0 && imageObject)
            {
                // While the image is not fully revealed, keep zooming out
                while (revealTime > 0)
                {
                    // Reduce the reveal time
                    revealTime -= Time.deltaTime;

                    // Wait for update. This is used to allow animation
                    yield return new WaitForFixedUpdate();

                    // Gradually reveal the full image (animated)
                    imageObject.offsetMin = Vector2.Lerp(imageObject.offsetMin, Vector2.zero, Time.deltaTime * 10);
                    imageObject.offsetMax = Vector2.Lerp(imageObject.offsetMax, Vector2.zero, Time.deltaTime * 10);
                }

                // Make sure the image is fully revealed
                imageObject.offsetMin = Vector2.zero;
                imageObject.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// Plays the sound attached to this question
        /// </summary>
        public void PlayQuestionSound()
        {
            if ( askingQuestion == true )
            {
                // If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
                if (soundPlayTime <= 0 && soundSourceTag != string.Empty && questions[currentQuestion].sound)
                {
                    // Play the sound
                    GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(questions[currentQuestion].sound);

                    // Set the play sound time, during which no other sounds will play
                    soundPlayTime = questions[currentQuestion].sound.length;
                }

            }
            else
            {
                // If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
                if (soundPlayTime <= 0 && soundSourceTag != string.Empty && questions[currentQuestion].followupSound)
                {
                    // Play the sound
                    GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(questions[currentQuestion].followupSound);

                    // Set the play sound time, during which no other sounds will play
                    soundPlayTime = questions[currentQuestion].followupSound.length;
                }
            }
        }
        

        /// <summary>
        /// Plays the sound attached to this answer based on its index
        /// </summary>
        public void PlayAnswerSound(int answerIndex)
        {
            // If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
            if (soundPlayTime <= 0 && soundSourceTag != string.Empty && questions[currentQuestion].answers[answerIndex].sound)
            {
                // Play the sound
                GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(questions[currentQuestion].answers[answerIndex].sound);

                // Set the play sound time, during which no other sounds will play
                soundPlayTime = questions[currentQuestion].answers[answerIndex].sound.length;
            }

        }

        /// <summary>
        /// Gives the player an extra life once per match. This can be used with UnityAds to reward the player when watching an ad
        /// </summary>
        public void ExtraLife()
        {
            // Add to lives
            players[currentPlayer].lives++;

            // Update the lives we have left
            Updatelives();

            // The game is not over anymore
            isGameOver = false;

            // Reset the score texts and hide the gameover object
            if (gameOverCanvas)
            {
                gameOverCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text = gameOverCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text.Substring(0, gameOverCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text.Length - players[currentPlayer].score.ToString().Length);

                gameOverCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text = gameOverCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text.Substring(0, gameOverCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text.Length - highScore.ToString().Length);

                gameOverCanvas.gameObject.SetActive(false);
            }

            // Ask the next question
            StartCoroutine(AskQuestion(false));
        }

        /// <summary>
        /// Loads an image from an online address and displays it on the relevant Image object
        /// </summary>
        /// <param name="imageURL"></param>
        /// <param name="targetImage"></param>
        /// <param name="enlargeButton"></param>
        /// <returns></returns>
        public IEnumerator LoadImageFromURL( string imageURL, Image targetImage, bool enlargeButton)
        {
            // Stop the timer while we load the image
            timerRunning = false;

            // Hide the sprite object while we load the image
            targetImage.gameObject.SetActive(false);

            // Get the address of the image
            WWW wwwLoader = new WWW(imageURL);

            // Wait until it's loaded
            yield return wwwLoader;

            // Display it in the question image
            targetImage.GetComponent<Image>().sprite = Sprite.Create(wwwLoader.texture, new Rect(0, 0, wwwLoader.texture.width, wwwLoader.texture.height), new Vector2(0, 0));

            // Show the sprite object ( that holds the image )
            targetImage.gameObject.SetActive(true);

            // If this is the question image, add an Enlarge button to it
            if ( enlargeButton == true )
            {
                // Add a button to the image, so that we can enlarge it when we click it
                if (questionObject.GetComponent<Button>() == null) questionObject.gameObject.AddComponent<Button>();

                // Listen for a click on the button to enlarge the image in it
                questionObject.GetComponent<Button>().onClick.AddListener(delegate () { ShowLargerImage(largerImageCanvas); });

                // Enable the button from the question, so that we can click it to open a larger image
                if (questionObject.GetComponent<Button>()) questionObject.GetComponent<Button>().enabled = true;

            }

            // Start the timer again
            timerRunning = true;
        }
        
        /// <summary>
        /// Loads a sound from an online address and assigns it to the relevant question/answer field
        /// </summary>
        /// <param name="soundURL"></param>
        /// <param name="targetQuestion"></param>
        /// <param name="answerObject"></param>
        /// <param name="targetAnswer"></param>
        /// <returns></returns>
        public IEnumerator LoadSoundFromURL(string soundURL, Question targetQuestion, Transform answerObject, Answer targetAnswer)
        {
            // Stop the timer while we load the image
            timerRunning = false;

            // Deactivate the sound button, since it may have been hidden before
            if ( targetQuestion != null ) questionObject.Find("ButtonPlaySound").gameObject.SetActive(false);
            if ( targetAnswer != null ) answerObject.Find("ButtonPlaySound").gameObject.SetActive(false);

            // Get the address of the image
            WWW wwwLoader = new WWW(soundURL);

            // Wait until it's loaded
            yield return wwwLoader;

            if (targetQuestion != null)
            {
                // Display it in the question image
                targetQuestion.sound = wwwLoader.GetAudioClip();

                // Activate the sound button, since it may have been hidden before
                questionObject.Find("ButtonPlaySound").gameObject.SetActive(true);

                // Play the sound when the question is displayed
                if (questionLimitCount < questionLimit || questionLimit == 0) PlayQuestionSound();
            }

            if ( answerObject && targetAnswer != null )
            {
                // Display it in the question image
                targetAnswer.sound = wwwLoader.GetAudioClip();

                // Activate the sound button, since it may have been hidden before
                answerObject.Find("ButtonPlaySound").gameObject.SetActive(true);
            }

            // Start the timer again
            timerRunning = true;
        }
    }
}