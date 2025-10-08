using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class ListSelectElement
    {
        VisualElement m_DocumentRoot;
        private List<string> answerChoices;
        ListView m_ListView = new ListView();
        VisualElement m_boxForScrollView = new VisualElement();
        public string Value = string.Empty;
        Button m_openButton;
        Label m_question_label = new Label();
        bool visible = false;
        public ListSelectElement(VisualElement root, Button openButton, string question)
        {
            m_DocumentRoot = root;
            m_openButton = openButton;
            openButton.clicked += () => AddToContext();
            //Because making this to a MonoBehaviour is overkill, we implement our own VisualTree-"Method" and get the topmost visualelement
            VisualElement prevRoot = m_DocumentRoot;
            while (m_DocumentRoot != null)
            {
                prevRoot = m_DocumentRoot;
                m_DocumentRoot = m_DocumentRoot.parent;
            }
            m_DocumentRoot = prevRoot;
            m_question_label.text = question;
            m_boxForScrollView.Add(m_question_label);
            m_boxForScrollView.Add(m_ListView);
            SetStyle();
            //Prepare the ListView
            m_ListView.makeItem = () =>
            {
                var newAnswer = new Label();
                newAnswer.style.whiteSpace = WhiteSpace.Normal;
                return newAnswer;
            };
            m_ListView.bindItem = (item, index) =>
            {
                (item as Label).text = answerChoices[index];
            };
            m_ListView.itemsChosen += (obj) =>
            ObjSelectionChanged(obj);
        }

        private void ObjSelectionChanged(IEnumerable<object> obj)
        {
            Value = obj.FirstOrDefault().ToString();
            m_openButton.text = Value;
            RemoveFromContext();
        }

        private void SetStyle()
        {
            m_boxForScrollView.style.top = 0;
            m_boxForScrollView.style.bottom = 0;
            m_boxForScrollView.style.left = 0;
            m_boxForScrollView.style.right = 0;
            m_boxForScrollView.style.position = Position.Absolute;
            m_boxForScrollView.style.backgroundColor = Color.gray;
            m_ListView.style.alignSelf = Align.Center;
            m_ListView.style.fontSize = 24;
            m_ListView.style.marginBottom = 25;
            m_ListView.style.marginTop = 25;
            m_ListView.style.marginLeft = 25;
            m_ListView.style.marginRight = 25;
            m_ListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            m_ListView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            m_question_label.style.fontSize = 29;
            m_question_label.style.alignSelf = Align.Center;
            m_question_label.style.whiteSpace = WhiteSpace.Normal;
            m_openButton.style.whiteSpace = WhiteSpace.Normal;
        }
        public List<string> AnswerChoices
        {
            get => answerChoices;
            set
            {
                answerChoices = value;
                answerChoices.Shuffle();
                m_ListView.itemsSource = answerChoices;
            }
        }

        public void AddToContext()
        {
            if (!visible)
                m_DocumentRoot.Add(m_boxForScrollView);
            visible = true;
        }
        public void RemoveFromContext()
        {
            if (visible)
                m_DocumentRoot?.Remove(m_boxForScrollView);
            visible = false;
        }
        public void SetEnabled(bool enabled)
        {
            RemoveFromContext();
            m_openButton.SetEnabled(enabled);
        }
    }
}