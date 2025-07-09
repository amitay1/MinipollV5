/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.1
 * Copyright 2014 - 2017 Fantasieleben UG (haftungsbeschraenkt)
 *
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman Geishauser: tilman@fantasieleben.com
 */

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace NEEDSIMSampleSceneScripts
{
    /// <summary>
    /// public methods to switch scenes via a button click. You have to add the scenes to your build settings to use the prefab that uses this script.
    /// </summary>
    public class SceneSwitcher : MonoBehaviour
    {
        string[] levelNames = { "01 Naturleben", "02 Hasenjagd", "03 Fuchsalarm", "04 Simple Room", "05 Spawn Beds", "06 Simple Time", "07 Ownership", "08 Resource Management", "09 Evaluation Callback", "10 Agent death on low need satisfaction" };

        public void loadLevel00()
        {
            SceneManager.LoadScene(levelNames[0]);
        }

        public void loadLevel01()
        {
            SceneManager.LoadScene(levelNames[1]);
        }

        public void loadLevel02()
        {
            SceneManager.LoadScene(levelNames[2]);
        }

        public void loadLevel03()
        {
            SceneManager.LoadScene(levelNames[3]);
        }

        public void loadLevel04()
        {
            SceneManager.LoadScene(levelNames[4]);
        }

        public void loadLevel05()
        {
            SceneManager.LoadScene(levelNames[5]);
        }

        public void loadLevel06()
        {
            SceneManager.LoadScene(levelNames[6]);
        }

        public void loadLevel07()
        {
            SceneManager.LoadScene(levelNames[7]);
        }

        public void loadLevel08()
        {
            SceneManager.LoadScene(levelNames[8]);
        }

        public void loadLevel09()
        {
            SceneManager.LoadScene(levelNames[9]);
        }
    }
}