/**
 * This is part of the NEEDSIM Life simulation plug in for Unity, version 1.2.1
 * Copyright 2014 - 2021 Fantasieleben 
 * 
 * http;//www.fantasieleben.com for further details.
 *
 * For questions please get in touch with Tilman: fantasieleben@mailbox.org
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NEEDSIMSampleSceneScripts
{
    /// <summary>
    /// Spawns a UI Element for each need and each satisfaction rate of an interaction.
    /// </summary>
    public class SpawnUIRuntimeEditing : MonoBehaviour
    {
        [SerializeField]
        private GameObject UIElementPrefab;

        [SerializeField]
        private GameObject ParentNeedsColumn;
        [SerializeField]
        private GameObject ParentInteractionsColumn;

        private GameObject[] NeedUIElements;
        private GameObject[] InteractionUIElements;

        private const string descriptionTag = "Description";
        private const string valueFieldTag = "Value";

        // Use this for initialization
        void Start()
        {
            NeedUIElements = new GameObject[Simulation.Manager.Instance.Data.NeedNames.Count];

            #region Determine length of InteractionUIElements[]
            int interactionUIElementsCount = 0;
            foreach (KeyValuePair<string, Simulation.Interaction> interaction in Simulation.Manager.Instance.Data.InteractionByNameDictionary)
            {
                foreach (KeyValuePair<string, float> satisfactionRate in interaction.Value.SatisfactionRates)
                {
                    if (satisfactionRate.Value != 0)
                    {
                        interactionUIElementsCount++;
                    }
                }
            }
            #endregion
            InteractionUIElements = new GameObject[interactionUIElementsCount];

            #region Spawn an InputFieldRuntimeEditing element for each need.
            int i = 0;

            foreach (string need in Simulation.Manager.Instance.Data.NeedNames)
            {
                NeedUIElements[i] = GameObject.Instantiate(UIElementPrefab);
                NeedUIElements[i].transform.SetParent(ParentNeedsColumn.transform, false);

                InputFieldRuntimeEditing inputField = NeedUIElements[i].GetComponent<InputFieldRuntimeEditing>();
                inputField.valueType = InputFieldRuntimeEditing.TypeOfValue.NeedChangeRate;

                UnityEngine.UI.Text[] textFields = NeedUIElements[i].GetComponentsInChildren<UnityEngine.UI.Text>();
                for (int j = 0; j < textFields.Length; j++)
                {
                    if (textFields[j].text == descriptionTag)
                    {
                        textFields[j].text = need + " change per second";
                        inputField.needName = need;
                    }
                    if (textFields[j].text == valueFieldTag)
                    {
                        textFields[j].text = Simulation.Manager.Instance.Data.ChangePerSecond[need].ToString();
                        inputField.value = Simulation.Manager.Instance.Data.ChangePerSecond[need];
                    }
                }
                i++;
            }
            #endregion

            #region Spawn an InputFieldRuntimeEditing for each satisfaction rate of each interaction
            i = 0;
            foreach (KeyValuePair<string, Simulation.Interaction> interaction in Simulation.Manager.Instance.Data.InteractionByNameDictionary)
            {
                foreach (KeyValuePair<string, float> satisfactionRate in interaction.Value.SatisfactionRates)
                {
                    if (satisfactionRate.Value != 0)
                    {
                        InteractionUIElements[i] = GameObject.Instantiate(UIElementPrefab);
                        InteractionUIElements[i].transform.SetParent(ParentInteractionsColumn.transform, false);

                        InputFieldRuntimeEditing inputField = InteractionUIElements[i].GetComponent<InputFieldRuntimeEditing>();
                        inputField.valueType = InputFieldRuntimeEditing.TypeOfValue.InteractionSatisfactionRate;

                        UnityEngine.UI.Text[] textFields = InteractionUIElements[i].GetComponentsInChildren<UnityEngine.UI.Text>();
                        for (int j = 0; j < textFields.Length; j++)
                        {
                            if (textFields[j].text == descriptionTag)
                            {
                                textFields[j].text = interaction.Key + " changes " + satisfactionRate.Key;
                                inputField.interaction = interaction.Key;
                                inputField.interactionNeed = satisfactionRate.Key;
                            }
                            if (textFields[j].text == valueFieldTag)
                            {
                                textFields[j].text = satisfactionRate.Value.ToString();
                                inputField.value = satisfactionRate.Value;
                            }
                        }
                        i++;
                    }
                }
            }
            #endregion
        }
    }
}
