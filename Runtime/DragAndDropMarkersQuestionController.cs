using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class DragAndDropMarkersQuestionController : AbstractQuestionController<DragAndDropMarkersQuestion>
    {
        List<VisualElement> m_DraggableElements = new List<VisualElement>();
        List<VisualElement> m_Dropzones = new List<VisualElement>();
        List<VisualElement> m_HiddenDropzones = new List<VisualElement>();
        List<int> m_DragsThatNeedAssigning = new List<int>();
        VisualElement m_InteractionElement;

        public DragAndDropMarkersQuestionController(DragAndDropMarkersQuestion question, VisualElement root = null, VisualTreeAsset dragAndDropMarkersQuestionTemplate = null)
        {

            m_Root = root;
            m_Question = question;
            m_QuestionTemplate = dragAndDropMarkersQuestionTemplate;
        }
        override public ScrollView ConstructQuestionText()
        {
            if (m_QuestionText != null)
                return m_QuestionText;
            if (m_QuestionBackground != null)
                m_QuestionText = m_QuestionBackground.Q<ScrollView>("question-text");
            if (m_QuestionText == null)
                m_QuestionText = new ScrollView();
            VisualElement currentTextRow = new VisualElement();
            currentTextRow.AddToClassList("question-text-row");
            m_QuestionText.Add(currentTextRow);
            Regex dropZonesRegex = new Regex("\\[\\[\\d*?\\]\\]");
            string[] words = m_Question.Questiontext.Text.Split(' ');
            foreach (string word in words)
            {
                int lastIndex = 0;
                foreach (Match match in dropZonesRegex.Matches(word))
                {
                    if (match.Index > lastIndex)
                    {
                        string wordToAdd = word.Substring(lastIndex, match.Index - lastIndex);
                        currentTextRow = AddWord(wordToAdd, m_QuestionText, currentTextRow);
                    }
                    Label hiddenDropZone = new Label();
                    hiddenDropZone.AddToClassList("hideText");
                    m_HiddenDropzones.Add(hiddenDropZone);
                    currentTextRow.Add(hiddenDropZone);
                    OuterGlow dropZone = new OuterGlow();
                    dropZone.userData = GenerateDropZoneData(match.Value);
                    dropZone.AddToClassList("dropZone");
                    dropZone.AddToClassList("markers");
                    dropZone.name = "dropZoneMarkers";
                    dropZone.style.position = Position.Absolute;
                    hiddenDropZone.RegisterCallback<GeometryChangedEvent>((GeometryChangedEvent evt) => ResizeDrop(evt.target as VisualElement, dropZone, currentTextRow));
                    currentTextRow.Add(dropZone);
                    m_Dropzones.Add(dropZone);
                    lastIndex = match.Index + match.Length;
                }
                if (lastIndex < word.Length)
                {
                    string wordToAdd = word.Substring(lastIndex);
                    currentTextRow = AddWord(wordToAdd, m_QuestionText, currentTextRow);
                }

            }
            return null;

        }
        override public VisualElement ConstructInteractionElement()
        {
            if (m_InteractionElement != null)
                return m_InteractionElement;
            ConstructQuestionText();
            VisualElement draggables = null;
            if (m_Root != null)
                draggables = m_Root.Q<VisualElement>("draggablesMarkers");
            int maxWith = 0;
            m_Question.DragBoxes.Shuffle();
            int index = 0;
            foreach (var drag in m_Question.DragBoxes)
            {
                VisualElement draggableElementContainer = new VisualElement();
                draggableElementContainer.name = index.ToString();
                index++;
                draggableElementContainer.AddToClassList("draggableElementContainer");
                Label draggableElement = new Label();
                draggableElement.text = drag.Text;

                draggableElement.userData = drag;
                draggableElement.AddToClassList("draggableElement");
                draggableElement.name = "draggableElement";
                draggableElementContainer.Add(draggableElement);
                maxWith = math.max(maxWith, drag.Text.Length);
                draggables.Add(draggableElementContainer);
                DragAndDropManipulator manipulator = new(draggableElement, true, m_QuestionText);
                draggableElementContainer.userData = manipulator;
                m_DraggableElements.Add(draggableElement);
                draggableElementContainer.RegisterCallback<GeometryChangedEvent>((GeometryChangedEvent evt) => ResizeHiddenDropzones(evt.target as VisualElement));


            }
            return m_InteractionElement = draggables;
        }

        private VisualElement AddWord(string wordToAdd, VisualElement questionText, VisualElement currentTextRow)
        {
            List<string> result = new List<string>();
            Regex newLineRegex = new Regex("\\n");
            int newLineLastIndex = 0;
            foreach (Match newLineMatch in newLineRegex.Matches(wordToAdd))
            {
                if (newLineMatch.Index > newLineLastIndex)
                {
                    Label preText = new Label();
                    preText.text = wordToAdd.Substring(newLineLastIndex, newLineMatch.Index - newLineLastIndex);
                    preText.AddToClassList("question-text-row-label");
                    currentTextRow.Add(preText);
                }
                currentTextRow = new VisualElement();
                currentTextRow.AddToClassList("question-text-row");
                questionText.Add(currentTextRow);
                newLineLastIndex = newLineMatch.Index + newLineMatch.Length;

            }
            if (newLineLastIndex < wordToAdd.Length)
            {
                Label postText = new Label();
                postText.AddToClassList("question-text-row-label");
                postText.text = wordToAdd.Substring(newLineLastIndex);
                currentTextRow.Add(postText);
            }
            return currentTextRow;
        }

        private Vector2 maxSize = new Vector2();
        private void ResizeHiddenDropzones(VisualElement visualElement)
        {
            foreach (var draggableElement in m_DraggableElements)
            {
                var parent = draggableElement.parent;
                maxSize.x = math.max(maxSize.x, parent.worldBound.width);
                maxSize.y = math.max(maxSize.y, parent.worldBound.height);
            }
            foreach (var hiddenDropzone in m_HiddenDropzones)
            {
                hiddenDropzone.style.width = maxSize.x;
                hiddenDropzone.style.height = maxSize.y;
            }
        }

        private void ResizeDrop(VisualElement hiddenDropZone, VisualElement dropZone, VisualElement currentTextRow)
        {
            var localPos = hiddenDropZone.parent.WorldToLocal(hiddenDropZone.worldBound);
            dropZone.style.left = localPos.x;
            dropZone.style.top = localPos.y;
            dropZone.style.width = localPos.width;
            dropZone.style.height = localPos.height;
        }


        protected override void Grade()
        {
            m_Result = new QuizResult();
            m_Result.MaxPoints = m_DraggableElements.Count;
            foreach (var element in m_DraggableElements)
            {
                var dragElement = (element.userData as DragBox);
                element.SetEnabled(false);
                var dropZone = element.parent.parent;
                if (dropZone.name != "dropZoneMarkers")
                {
                    if (m_DragsThatNeedAssigning.Contains(dragElement.Index))
                    {
                        //element.parent.AddToClassList("false-answer");
                        element.AddToClassList("false-answer");
                    }
                    else
                    {
                        m_Result.Points++;
                        //element.parent.AddToClassList("correct-answer");
                        element.AddToClassList("correct-answer");
                    }
                    continue;
                }
                var dropZoneData = dropZone.userData as DropZoneData;
                if (dropZoneData.FittingDragElementIDs.Contains(dragElement.Index))
                {
                    //element.parent.AddToClassList("correct-answer");
                    element.AddToClassList("correct-answer");
                    m_Result.Points++;
                }
                else
                {
                    //element.parent.AddToClassList("false-answer");
                    element.AddToClassList("false-answer");
                }
            }
        }
        protected override void DisableQuestion()
        {
            m_SubmitButton.style.display = DisplayStyle.Flex;
            m_SubmitButton.clicked += ShowSolution;
            m_SubmitButton.text = "Lösung anzeigen";
        }

        private DropZoneData GenerateDropZoneData(string matchedValue)
        {
            var data = new DropZoneData();
            int fittingID = int.Parse(matchedValue.Substring(2, matchedValue.Length - 4));
            m_DragsThatNeedAssigning.Add(fittingID);
            data.FittingDragElementIDs.Add(fittingID);
            foreach (DragBox dragBox in m_Question.DragBoxes)
            {
                if (dragBox.Index == fittingID)
                {
                    data.Group = dragBox.Group;
                    break;
                }
            }
            return data;
        }

        Dictionary<VisualElement, List<VisualElement>> m_PlayerSolutionState;
        Dictionary<VisualElement, List<VisualElement>> m_CorrectSolutionState;
        private void ShowSolution()
        {
            m_SubmitButton.clicked -= ShowSolution;
            m_SubmitButton.text = "Eigene Eingabe anzeigen";
            m_SubmitButton.clicked += ShowPlayerSolution;
            if (m_PlayerSolutionState == null)
                SavePlayerSolutionState();
            if (m_CorrectSolutionState == null)
                GenerateCorrectSolutionState();
            foreach (var draggables in m_DraggableElements)
            {
                draggables.EnableInClassList("solution", true);
            }
            CreateState(m_CorrectSolutionState);
        }

        private void ShowPlayerSolution()
        {
            m_SubmitButton.clicked -= ShowPlayerSolution;
            m_SubmitButton.text = "Lösung anzeigen";
            m_SubmitButton.clicked += ShowSolution;
            if (m_PlayerSolutionState == null)
                SavePlayerSolutionState();
            if (m_CorrectSolutionState == null)
                GenerateCorrectSolutionState();
            foreach (var draggables in m_DraggableElements)
            {
                draggables.EnableInClassList("solution", false);
            }
            CreateState(m_PlayerSolutionState);

        }

        private void CreateState(Dictionary<VisualElement, List<VisualElement>> state)
        {
            foreach (var tuple in state)
            {
                foreach (var draggable in tuple.Value)
                {
                    tuple.Key.Add(draggable);
                }
            }
        }


        private void GenerateCorrectSolutionState()
        {
            m_CorrectSolutionState = new Dictionary<VisualElement, List<VisualElement>>();
            foreach (var dropZone in m_Dropzones)
            {
                UQueryBuilder<VisualElement> dragElementQuery = dropZone.Query<VisualElement>(className: "draggableElementContainer");
                DropZoneData dropZoneData = dropZone.userData as DropZoneData;
                m_CorrectSolutionState[dropZone] = m_DraggableElements.FindAll((element) =>
                {
                    DragBox dragBox = element.userData as DragBox;
                    return dropZoneData.FittingDragElementIDs.Contains(dragBox.Index);
                }).Select((element) => element.parent).ToList();
            }
            var draggables = m_Root.Q<VisualElement>("draggablesMarkers");
            m_CorrectSolutionState[draggables] = m_DraggableElements.FindAll((element) =>
            {
                DragBox dragBox = element.userData as DragBox;
                return !m_DragsThatNeedAssigning.Contains(dragBox.Index);
            }).Select((element) => element.parent).ToList();
        }

        private void SavePlayerSolutionState()
        {
            m_PlayerSolutionState = new Dictionary<VisualElement, List<VisualElement>>();
            foreach (var dropZone in m_Dropzones)
            {
                UQueryBuilder<VisualElement> dragElementQuery = dropZone.Query<VisualElement>(className: "draggableElementContainer");
                m_PlayerSolutionState[dropZone] = dragElementQuery.ToList();
            }
            var draggables = m_Root.Q<VisualElement>("draggablesMarkers");
            UQueryBuilder<VisualElement> draggablesQuery = draggables.Query<VisualElement>(className: "draggableElementContainer");
            m_PlayerSolutionState[draggables] = draggablesQuery.ToList();

        }

    }
}