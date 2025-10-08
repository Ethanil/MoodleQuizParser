using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class DragAndDropQuestionController : AbstractQuestionController<DragAndDropQuestion>
    {
        StreamlinedDrops m_StreamlinedDrops;
        List<VisualElement> m_DraggableElements = new List<VisualElement>();
        List<VisualElement> m_Dropzones = new List<VisualElement>();
        List<int> m_DragsThatNeedAssigning = new List<int>();
        VisualElement m_InteractionElement;
        public DragAndDropQuestionController(DragAndDropQuestion question, VisualElement root = null, VisualTreeAsset dragAndDropQuestionTemplate = null)
        {
            m_Root = root;
            m_Question = question;
            m_StreamlinedDrops = new StreamlinedDrops(question.Drops);
            m_QuestionTemplate = dragAndDropQuestionTemplate;
        }

        override public ScrollView ConstructQuestionText()
        {
            if (m_QuestionText != null)
                return m_QuestionText;
            if (m_Root != null)
                m_QuestionText = m_Root.Q<ScrollView>("question-text");
            if (m_QuestionText == null)
                m_QuestionText = new ScrollView();
            QuizUtility.GenerateQuestionText(m_Question.Questiontext, m_QuestionText);

            return m_QuestionText;

        }
        override public VisualElement ConstructInteractionElement()
        {
            if (m_InteractionElement != null)
                return m_InteractionElement;
            if (m_Root != null)
                m_InteractionElement = m_Root.Q<VisualElement>("drag-and-drop");
            if (m_InteractionElement == null)
                m_InteractionElement = new VisualElement();
            var image_container = m_InteractionElement.Q<VisualElement>("image-container");
            if (image_container == null)
                image_container = new VisualElement();
            VisualElement draggables = null;
            if (m_Root != null)
                draggables = m_Root.Q<VisualElement>("draggables");
            if (draggables == null)
                draggables = new VisualElement();

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(m_Question.File.Data);
            backgroundImage.style.backgroundImage = tex;
            backgroundImage.AddToClassList("backgroundImage");
            var image = new Image();
            image.image = tex;
            image_container.Add(backgroundImage);
            image.AddToClassList("image");
            foreach (var drop in m_StreamlinedDrops.GetShapes())
            {
                var shape = drop.Shape;
                var dropShapeType = shape.GetType();
                if (dropShapeType == typeof(Rectangle))
                {
                    var rectangle = (Rectangle)shape;

                    var dropZone = new OuterGlow();
                    image_container.Add(dropZone);
                    dropZone.AddToClassList("dropZone");
                    dropZone.name = "dropZone";
                    m_Dropzones.Add(dropZone);
                    dropZone.userData = GenerateDropZoneData(drop.FittingDragElementIDs);
                    m_DragsThatNeedAssigning.AddRange(drop.FittingDragElementIDs);
                    image_container.RegisterCallback<GeometryChangedEvent>((GeometryChangedEvent evt) => ResizeDrop(evt.target as VisualElement, dropZone, rectangle, tex.width, tex.height));
                }
            }
            int index = 0;
            m_Question.Drags.Shuffle();
            foreach (var drag in m_Question.Drags)
            {
                int numberOfDragElements = 1;
                if (drag.GetType() == typeof(DragElementWithMaxUses))
                {
                    numberOfDragElements = (drag as DragElementWithMaxUses).NumberOfDrags;
                }
                for (int i = 0; i < numberOfDragElements; i++)
                {
                    index++;
                    VisualElement draggableElementContainer = new VisualElement();
                    draggableElementContainer.AddToClassList("draggableElementContainer");
                    VisualElement draggableElement;
                    if (drag.GetType() == typeof(DragElementWithImage) && (drag as DragElementWithImage).File != null && (drag as DragElementWithImage).File.Data.Length > 0)
                    {
                        DragElementWithImage dragImage = (DragElementWithImage)drag;
                        draggableElement = new Image();
                        Texture2D t = new Texture2D(2, 2);
                        t.LoadImage((drag as DragElementWithImage).File.Data);
                        (draggableElement as Image).image = t;
                    }
                    else
                    {
                        draggableElement = new Label();
                        (draggableElement as Label).text = drag.Text;

                    }
                    draggableElement.userData = drag;
                    draggableElement.AddToClassList("draggableElement");
                    draggableElement.name = "draggableElement";
                    draggableElementContainer.Add(draggableElement);
                    draggables.Add(draggableElementContainer);
                    DragAndDropManipulator manipulator = new(draggableElement, m_StreamlinedDrops.GetShapes().First().IsSingleShot);
                    draggableElementContainer.userData = manipulator;
                    m_DraggableElements.Add(draggableElement);
                }


            }
            return m_InteractionElement;
        }



        protected override void Grade()
        {
            m_Result = new QuizResult();
            m_Result.MaxPoints = m_DraggableElements.Count;
            foreach (var element in m_DraggableElements)
            {
                var dragElement = (element.userData as DragElement);
                element.SetEnabled(false);
                var dropZone = element.parent.parent;
                if (dropZone.name == "dropZone" && (dropZone.userData as DropZoneData).FittingDragElementIDs.Contains(dragElement.Number))
                {
                    element.AddToClassList("correct-answer");
                    m_Result.Points++;
                }
                else
                {
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

        VisualElement backgroundImage = new VisualElement();
        override protected void FillQuestionWithData()
        {
            if (m_Question.File == null || m_Question.File.Data.Length == 0)
                return;

            base.FillQuestionWithData();

        }

        private void ResizeDrop(VisualElement target, VisualElement dropzone, Rectangle rectangle, int imageWidth, int imageHeight)
        {
            // because i could not find any other way to scale other stuff the same way the background image scale to fit scaled i reimplemented it

            // get the smaller of the 2 scale factors to let the image fit into the content
            var scaleFactor = Math.Min(target.resolvedStyle.height / imageHeight, target.resolvedStyle.width / imageWidth);

            // scale the rectangle(the dropZone)
            int scaledRectangleTopLeftX = (int)(rectangle.TopLeft.X * scaleFactor);
            int scaledRectangleTopLeftY = (int)(rectangle.TopLeft.Y * scaleFactor);
            int scaledRectangleWidth = (int)(rectangle.Width * scaleFactor);
            int scaledRectangleHeight = (int)(rectangle.Height * scaleFactor);
            // get the scaled image sizes (the visualElement does have other width/height)
            int scaledImageWidth = (int)(imageWidth * scaleFactor);
            int scaledImageHeight = (int)(imageHeight * scaleFactor);
            // get the translation from all sides - this breaks if we don't center the image in the container
            int translationX = (int)((target.resolvedStyle.width - scaledImageWidth) / 2);
            int translationY = (int)((target.resolvedStyle.height - scaledImageHeight) / 2);
            // finally scale the dropzone
            dropzone.style.left = scaledRectangleTopLeftX + translationX;
            dropzone.style.top = scaledRectangleTopLeftY + translationY;
            dropzone.style.right = scaledImageWidth + translationX - scaledRectangleTopLeftX - scaledRectangleWidth;
            dropzone.style.bottom = scaledImageHeight + translationY - scaledRectangleTopLeftY - scaledRectangleHeight;
        }
        private DropZoneData GenerateDropZoneData(List<int> fittingIds)
        {
            var data = new DropZoneData();
            data.FittingDragElementIDs.AddRange(fittingIds);
            if (m_Question.Drags.First().GetType() == typeof(DragElementWithMaxUses))
            {
                return data;
            }
            foreach (DragElementWithImage dragElement in m_Question.Drags)
            {
                if (fittingIds.Contains(dragElement.Number))
                {
                    data.Group = dragElement.DragGroup;
                    break;
                }
            }
            return data;
        }


        Dictionary<VisualElement, List<VisualElement>> m_PlayerSolutionState;
        Dictionary<VisualElement, List<VisualElement>> m_CorrectSolutionState;
        private void ShowSolution()
        {
            if (m_SubmitButton != null)
            {
                m_SubmitButton.clicked -= ShowSolution;
                m_SubmitButton.text = "Eigene Eingabe anzeigen";
                m_SubmitButton.clicked += ShowPlayerSolution;
            }

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
            if (m_SubmitButton != null)
            {
                m_SubmitButton.clicked -= ShowPlayerSolution;
                m_SubmitButton.text = "Lösung anzeigen";
                m_SubmitButton.clicked += ShowSolution;
            }

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
                    DragElement dragBox = element.userData as DragElement;
                    return dropZoneData.FittingDragElementIDs.Contains(dragBox.Number);
                }).Select((element) => element.parent).ToList();
            }
            var draggables = m_Root.Q<VisualElement>("draggables");
            m_CorrectSolutionState[draggables] = m_DraggableElements.FindAll((element) =>
            {
                DragElement dragBox = element.userData as DragElement;
                return !m_DragsThatNeedAssigning.Contains(dragBox.Number);
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
            var draggables = m_Root.Q<VisualElement>("draggables");
            UQueryBuilder<VisualElement> draggablesQuery = draggables.Query<VisualElement>(className: "draggableElementContainer");
            m_PlayerSolutionState[draggables] = draggablesQuery.ToList();

        }

    }
}