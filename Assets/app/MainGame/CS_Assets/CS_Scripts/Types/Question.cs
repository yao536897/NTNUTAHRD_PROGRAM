﻿//Version 1.97 (11.11.2017)

using System;
using UnityEngine;
using TriviaQuizGame.Types;

namespace TriviaQuizGame.Types
{
	[Serializable]
	public class Question
	{
		[Tooltip("The question presented")]
		public string question;
		
		[Tooltip("An image that accompanies the question. You can leave this empty if you don't want an image")]
		public Sprite image;

        [Tooltip("The address of an image that is loaded through a URL. You can leave this empty if you don't want an image")]
        public string imageURL;

		[Tooltip("A sound that accompanies the question. You can leave this empty if you don't want a sound")]
		public AudioClip sound;

        [Tooltip("The address of a sound that accompanies the question. You can leave this empty if you don't want a sound")]
		public string soundURL;

        [Tooltip("A list of answers to choose from. A question may have several correct/wrong answers")]
        public Answer[] answers;

		[Tooltip("If true, the player must select all correct questions and confirm to check the result")]
		public bool multiChoice = false;

		[Tooltip("A followup text that will be displayed after this question is answered. While displayed, the game is paused.")]
		public String followup;

        [Tooltip("A followup image that will be displayed after this question is answered. While displayed, the game is paused.")]
        public Sprite followupImage;

        [Tooltip("The address of a followup image that will be displayed after this question is answered. While displayed, the game is paused.")]
        public string followupImageURL;

        [Tooltip("A followup sound that can be played after this question is answered. While displayed, the game is paused.")]
        public AudioClip followupSound;

        [Tooltip("The address of a followup sound that can be played after this question is answered. While displayed, the game is paused.")]
        public string followupSoundURL;

        [Tooltip("How many point we get if we answer this question correctly. The bonus value is also used to sort the questions from the easy ( low bonus ) to the difficult ( high bonus )")]
		public float bonus;
		
		[Tooltip("How many seconds to answer this question we have. This should logically be tied to the difficulty of the question, same as the bonus. But the questions are sorted only based on the bonus, and not the time")]
		public float time;
	}
}
