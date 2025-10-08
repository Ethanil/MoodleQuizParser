using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public partial class XMLParser
    {
        static public async Task<List<Question>> FixAndParseXML(string filepath)
        {
            var stream = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite);
            string result;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                string content = await reader.ReadToEndAsync();
                string textPattern = @"<text>(\n| )+";
                string textReplacement = "<text>";
                result = Regex.Replace(content, textPattern, textReplacement);
                string textEndPattern = @"(\n| )+</text>";
                string textEndReplacement = "</text>";
                result = Regex.Replace(result, textEndPattern, textEndReplacement);

                // make sure that all <text> tags are internally wrapped as CDATA (but only ones, to ensure that we use negative lookahead
                string pattern = @"<text>(?!<!\[CDATA\[)(.*?)(?<!\]\]>)<\/text>";
                string replacement = @"<text><![CDATA[$1]]></text>";
                result = Regex.Replace(result, pattern, replacement, RegexOptions.Singleline);
            }

            stream.Close();
            //stream.SetLength(0);
            //stream.Position = 0;
            stream = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite);
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(result);
                await writer.FlushAsync();
            }
            //stream.Position = 0;
            stream.Close();
            return await ParseXML(File.Open(filepath, FileMode.Open));
        }
        static public async Task<List<Question>> ParseXML(System.IO.Stream stream)
        {

            List<Question> result = new List<Question>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = false;
            using XmlReader reader = XmlReader.Create(stream, settings);
            reader.MoveToContent();
            int id = 0;
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Comment)
                {
                    var comment = reader.Value;
                    id = int.Parse(Regex.Replace(comment, "\\D", ""));
                    continue;
                }
                switch (reader.Name)
                {
                    case "xml":
                        // "Found quiz-node and skipped it
                        break;
                    case "quiz":
                        // "Found quiz-node and skipped it
                        break;
                    case "question":
                        var question = await ReadQuestion(reader, id);
                        if (question != null)
                            result.Add(question);
                        break;
                    default:
                        throw new FormatException($"Unknown outer Node found NodeName: {reader.Name} NodeValue: {await reader.GetValueAsync()}");
                }
            }
            stream.Close();
            return result;
        }

        static private async Task<Question> ReadQuestion(XmlReader reader, int id)
        {
            if (reader == null)
                throw new ArgumentNullException("XMLReader is null");
            if (!reader.HasAttributes)
                throw new FormatException("Found question Node without question-type");
            if (reader.AttributeCount > 1)
                throw new FormatException("Found question with multiple Attributes");

            switch (reader[0])
            {
                case "multichoice":
                    return await ReadMultipleChoiceQuestion(reader, id);
                case "category":
                    await SkipQuestion(reader);
                    return null;
                case "calculated":
                    return await ReadCalculatedQuestion(reader, id);
                case "calculatedmulti":
                    return await ReadCalculatedMultiQuestion(reader, id);
                case "calculatedsimple":
                    return await ReadCalculatedQuestion(reader, id);
                case "ddimageortext":
                    return await ReadDragAndDropQuestion(reader, id);
                case "ddmarker":
                    return await ReadDragAndDropMarkerQuestion(reader, id);
                case "ddwtos":
                    return await ReadDragAndDropInTextQuestion(reader, id);
                case "cloze":
                    return await ReadClozeQuestion(reader, id);
                case "description":
                    return await ReadBasicQuestion(reader, id);
                case "essay":
                    return await ReadEssayQuestion(reader, id);
                case "matching":
                    return await ReadMatchingQuestion(reader, id);
                case "numerical":
                    return await ReadNumericalQuestion(reader, id);
                case "pmatchjme":
                    return await ReadMolecularQuestion(reader, id);
                case "randomsamatch":
                    return await ReadRandomsMatchQuestion(reader, id);
                case "shortanswer":
                    return await ReadShortQuestion(reader, id);
                case "stack":
                    return await ReadStackQuestion(reader, id);
                case "truefalse":
                    return await ReadTrueFalseQuestion(reader, id);
                case "ordering":
                    return await ReadOrderingQuestion(reader, id);
                default:
                    throw new FormatException($"unknown Questiontype found: {reader[0]}");
            }
        }

        static private async Task SkipQuestion(XmlReader reader)
        {
            while (await reader.ReadAsync())
            {
                if (reader.Name == "question" && reader.NodeType == XmlNodeType.EndElement)
                    return;
            }
        }

        #region Base Question Functions
        static private async Task CheckBaseQuestionNodes(XmlReader reader, Question question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            switch (reader.Name)
            {
                case "name":
                    question.Name = await GetTextFromElement(reader, 2);
                    break;
                case "questiontext":
                    //question.Questiontext = await getTextFromElement(reader, 2);
                    question.Questiontext = await GetTextWithImages(reader, "questiontext");
                    break;
                case "generalfeedback":
                    question.GeneralFeedback = await GetTextWithImages(reader, "generalfeedback");
                    break;
                case "defaultgrade":
                    question.Defaultgrade = await GetFloatFromElement(reader);
                    break;
                case "penalty":
                    question.Penalty = await GetFloatFromElement(reader);
                    break;
                case "hidden":
                    question.Hidden = await GetBoolFromElement(reader);
                    break;
                case "idnumber":
                    question.IdNumber = await GetTextFromElement(reader);
                    break;
                case "tags":
                    await ReadTags(reader, question);
                    break;
            }
        }





        static private async Task CheckBaseMultipleQuestionNodes(XmlReader reader, BaseMultipleQuestion question)
        {
            switch (reader.Name)
            {
                case "single":
                    question.Single = await GetBoolFromElement(reader);
                    break;
                case "shuffleanswers":
                    question.ShuffleAnswers = await GetBoolFromElement(reader);
                    break;
                case "answernumbering":
                    question.Answernumbering = await GetTextFromElement(reader);
                    break;
                case "correctfeedback":
                    question.CorrectFeedback = await GetTextFromElement(reader, 2);
                    break;
                case "partiallycorrectfeedback":
                    question.PartiallyCorrectFeedback = await GetTextFromElement(reader, 2);
                    break;
                case "incorrectfeedback":
                    question.IncorrectFeedback = await GetTextFromElement(reader, 2);
                    break;
                case "shownumcorrect":
                    question.ShowNumCorrect = true;
                    break;
            }
        }

        static private async Task CheckUnitQuestionNodes(XmlReader reader, CalculatedQuestionWithUnit question)
        {
            switch (reader.Name)
            {
                case "unitgradingtype":
                    question.UnitGradingType = await GetIntFromElement(reader);
                    break;
                case "unitpenalty":
                    question.UnitPenalty = await GetFloatFromElement(reader);
                    break;
                case "showunits":
                    question.ShowUnits = await GetIntFromElement(reader);
                    break;
                case "unitsleft":
                    question.UnitsLeft = await GetBoolFromElement(reader);
                    break;
                case "units":
                    await ReadUnits(reader, question);
                    break;
            }
        }

        static private async Task CheckUnitQuestionNodes2(XmlReader reader, NumericQuestion question)
        {
            switch (reader.Name)
            {
                case "unitgradingtype":
                    question.UnitGradingType = await GetIntFromElement(reader);
                    break;
                case "unitpenalty":
                    question.UnitPenalty = await GetFloatFromElement(reader);
                    break;
                case "showunits":
                    question.ShowUnits = await GetIntFromElement(reader);
                    break;
                case "unitsleft":
                    question.UnitsLeft = await GetBoolFromElement(reader);
                    break;
                case "units":
                    await ReadUnits2(reader, question);
                    break;
            }
        }

        static private async Task ReadUnits(XmlReader reader, CalculatedQuestionWithUnit question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "units")
                    return;
                if (reader.Name != "unit")
                    throw new FormatException("unexpeted Unit-Nodes");
                Unit unit = new Unit();
                await reader.ReadAsync();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case "multiplier":
                            unit.Multiplier = await GetFloatFromElement(reader);
                            break;
                        case "unit_name":
                            unit.UnitName = await GetTextFromElement(reader);
                            break;
                        default:
                            throw new FormatException($"Unhandled Node while reading unit found NodeName: {reader.Name} NodeValue: {await reader.GetValueAsync()}");
                    }
                    await reader.ReadAsync();
                }
                question.Units.Add(unit);
            }
        }
        // We need a second readUnits function because NumericQuestion is not a CalculatedQuestionWithUnit, but also has the same Unit-Attributes
        static private async Task ReadUnits2(XmlReader reader, NumericQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "units")
                    return;
                if (reader.Name != "unit")
                    throw new FormatException("unexpeted Unit-Nodes");
                Unit unit = new Unit();
                await reader.ReadAsync();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case "multiplier":
                            unit.Multiplier = await GetFloatFromElement(reader);
                            break;
                        case "unit_name":
                            unit.UnitName = await GetTextFromElement(reader);
                            break;
                        default:
                            throw new FormatException($"Unhandled Node while reading unit found NodeName: {reader.Name} NodeValue: {await reader.GetValueAsync()}");
                    }
                    await reader.ReadAsync();
                }
                question.Units.Add(unit);
            }
        }

        static private async Task ReadTags(XmlReader reader, Question question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            if (reader.Name == "tags")
                reader.ReadAsync().Wait();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                question.Tags.Add(await GetTextFromElement(reader, 2));
                reader.ReadAsync().Wait();
            }
        }

        static private async Task CheckBasicAnswerNodes(XmlReader reader, Answer answer)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            switch (reader.Name)
            {
                case "answer":
                    if (!reader.HasAttributes || reader.GetAttribute("fraction") == null)
                        throw new FormatException("Answer Node has no Fraction");
                    answer.Fraction = float.Parse(reader["fraction"], CultureInfo.InvariantCulture);
                    break;
                case "text":
                    answer.Answertext = await GetTextFromElement(reader);
                    break;
                case "feedback":
                    answer.Feedback = await GetTextFromElement(reader, 2);
                    break;
            }
        }
        #endregion

        #region Multiple Choice Question Functions
        static private async Task<MultipleChoiceQuestion> ReadMultipleChoiceQuestion(XmlReader reader, int id)
        {
            MultipleChoiceQuestion question = new MultipleChoiceQuestion();
            question.QuestionID = id;
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;


                switch (reader.Name)
                {
                    case "showstandardinstruction":
                        question.ShowStandardinstruction = await GetBoolFromElement(reader);
                        break;
                    case "answer":
                        await ReadMultipleChoiceAnswer(reader, question);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadMultipleChoiceAnswer(XmlReader reader, MultipleChoiceQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            MultipleChoiceAnswer answer = new MultipleChoiceAnswer();
            // we need a do-while loop here because the first information is stored in an attribute instead as a text inside the node like in every other node
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.Answers.Add(answer); return; }
                await CheckBasicAnswerNodes(reader, answer);
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Calculated Question Functions
        static private async Task<CalculatedQuestionWithUnit> ReadCalculatedQuestion(XmlReader reader, int id)
        {
            CalculatedQuestionWithUnit question = new CalculatedQuestionWithUnit();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "answer":
                        await ReadCalculatedAnswer(reader, question);
                        break;
                    case "synchronize":
                        question.Synchronize = await GetIntFromElement(reader);
                        break;
                    case "dataset_definitions":
                        await ReadDataSetDefinitions(reader, question);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        await CheckUnitQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadCalculatedAnswer(XmlReader reader, CalculatedQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            CalculatedAnswer answer = new CalculatedAnswer();
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.Answers.Add(answer); return; }
                switch (reader.Name)
                {
                    case "tolerance":
                        answer.Tolerance = await GetFloatFromElement(reader);
                        break;
                    case "tolerancetype":
                        answer.ToleranceType = await GetIntFromElement(reader);
                        break;
                    case "correctanswerformat":
                        answer.CorrectAnswerFormat = await GetIntFromElement(reader);
                        break;
                    case "correctanswerlength":
                        answer.CorrectAnswerLength = await GetIntFromElement(reader);
                        break;
                    default:
                        await CheckBasicAnswerNodes(reader, answer);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadDataSetDefinitions(XmlReader reader, CalculatedQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "dataset_definitions")
                    return;
                if (reader.Name != "dataset_definition")
                    throw new FormatException("unexpeted dataset_definitions-Nodes");
                DatasetDefinition definition = new DatasetDefinition();
                await reader.ReadAsync();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case "status":
                            definition.Status = await GetTextFromElement(reader, 2);
                            break;
                        case "name":
                            definition.Name = await GetTextFromElement(reader, 2);
                            break;
                        case "type":
                            definition.Type = await GetTextFromElement(reader);
                            break;
                        case "distribution":
                            definition.Distribution = await GetTextFromElement(reader, 2);
                            break;
                        case "minimum":
                            definition.Minimum = await GetFloatFromElement(reader, 2);
                            break;
                        case "maximum":
                            definition.Maximum = await GetFloatFromElement(reader, 2);
                            break;
                        case "decimals":
                            definition.Decimals = await GetIntFromElement(reader, 2);
                            break;
                        case "itemcount":
                            definition.ItemCount = await GetIntFromElement(reader);
                            break;
                        case "dataset_items":
                            await ReadDataSetItems(reader, definition);
                            break;
                        case "number_of_items":
                            definition.ItemCount = await GetIntFromElement(reader);
                            break;
                        default:
                            throw new FormatException($"Unhandled Node while reading unit found NodeName: {reader.Name} NodeValue: {await reader.GetValueAsync()}");
                    }
                    await reader.ReadAsync();
                }
                question.DatasetDefinitions.Add(definition);
            }
        }

        static private async Task ReadDataSetItems(XmlReader reader, DatasetDefinition definition)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "dataset_items")
                    return;
                if (reader.Name != "dataset_item")
                    throw new FormatException("unexpeted dataset_item-Nodes");
                DatasetItem item = new DatasetItem();
                await reader.ReadAsync();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case "number":
                            item.Number = await GetIntFromElement(reader);
                            break;
                        case "value":
                            item.Value = await GetFloatFromElement(reader);
                            break;
                        default:
                            throw new FormatException($"Unhandled Node while reading unit found NodeName: {reader.Name} NodeValue: {await reader.GetValueAsync()}");
                    }
                    await reader.ReadAsync();
                }
                definition.DataSetItems.Add(item);
            }
        }
        #endregion

        #region Calculated Multi Question Functions
        static private async Task<CalculatedQuestion> ReadCalculatedMultiQuestion(XmlReader reader, int id)
        {
            CalculatedQuestion question = new CalculatedQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "answer":
                        await ReadCalculatedAnswer(reader, question);
                        break;
                    case "synchronize":
                        question.Synchronize = await GetIntFromElement(reader);
                        break;
                    case "dataset_definitions":
                        await ReadDataSetDefinitions(reader, question);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Drag and Drop Question Functions
        static private async Task<DragAndDropQuestion> ReadDragAndDropQuestion(XmlReader reader, int id)
        {
            DragAndDropQuestion question = new DragAndDropQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "drag":
                        await ReadDrag(reader, question);
                        break;
                    case "drop":
                        await ReadDrop(reader, question);
                        break;
                    case "file":
                        question.File = await GetMoodleFileFromElement(reader);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadDrag(XmlReader reader, DragAndDropQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            DragElementWithImage dragElement = new DragElementWithImage();
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "drag")
                { question.Drags.Add(dragElement); return; }
                switch (reader.Name)
                {
                    case "no":
                        dragElement.Number = await GetIntFromElement(reader);
                        break;
                    case "text":
                        dragElement.Text = await GetTextFromElement(reader);
                        break;
                    case "draggroup":
                        dragElement.DragGroup = await GetIntFromElement(reader);
                        break;
                    case "file":
                        dragElement.File = await GetMoodleFileFromElement(reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadDrop(XmlReader reader, DragAndDropQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            DropElementWithoutShape dropElement = new DropElementWithoutShape();
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "drop")
                { question.Drops.Add(dropElement); return; }
                switch (reader.Name)
                {
                    case "no":
                        dropElement.Number = await GetIntFromElement(reader);
                        break;
                    case "text":
                        dropElement.Text = await GetTextFromElement(reader);
                        break;
                    case "choice":
                        dropElement.Choice = await GetIntFromElement(reader);
                        break;
                    case "xleft":
                        dropElement.XLeft = await GetIntFromElement(reader);
                        break;
                    case "ytop":
                        dropElement.YTop = await GetIntFromElement(reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Drag and Drop Marker Question Functions
        static private async Task<DragAndDropQuestion> ReadDragAndDropMarkerQuestion(XmlReader reader, int id)
        {
            DragAndDropQuestion question = new DragAndDropQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "drag":
                        await ReadMarkerDrag(reader, question);
                        break;
                    case "drop":
                        await ReadMarkerDrop(reader, question);
                        break;
                    case "file":
                        question.File = await GetMoodleFileFromElement(reader);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadMarkerDrag(XmlReader reader, DragAndDropQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            DragElementWithMaxUses dragElement = new DragElementWithMaxUses();
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "drag")
                { question.Drags.Add(dragElement); return; }
                switch (reader.Name)
                {
                    case "no":
                        dragElement.Number = await GetIntFromElement(reader);
                        break;
                    case "text":
                        dragElement.Text = await GetTextFromElement(reader);
                        break;
                    case "noofdrags":
                        dragElement.NumberOfDrags = await GetIntFromElement(reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadMarkerDrop(XmlReader reader, DragAndDropQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            DropElementWithShape dropElement = new DropElementWithShape();
            string shape = "";
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "drop")
                { question.Drops.Add(dropElement); return; }
                switch (reader.Name)
                {
                    case "no":
                        dropElement.Number = await GetIntFromElement(reader);
                        break;
                    case "choice":
                        dropElement.Choice = await GetIntFromElement(reader);
                        break;
                    case "shape":
                        shape = await GetTextFromElement(reader);
                        break;
                    case "coords":
                        dropElement.Shape = await GetShapeFromElement(shape, reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Drag and Drop in Text Question Functions
        static private async Task<DragAndDropMarkersQuestion> ReadDragAndDropInTextQuestion(XmlReader reader, int id)
        {
            DragAndDropMarkersQuestion question = new DragAndDropMarkersQuestion();
            question.QuestionID = id;
            int dragBoxIndex = 1;
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "dragbox":
                        await ReadDragbox(reader, question, dragBoxIndex);
                        dragBoxIndex++;
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadDragbox(XmlReader reader, DragAndDropMarkersQuestion question, int dragBoxIndex)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            DragBox dragBox = new DragBox();
            dragBox.Index = dragBoxIndex;
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "dragbox")
                { question.DragBoxes.Add(dragBox); return; }
                switch (reader.Name)
                {
                    case "group":
                        dragBox.Group = await GetIntFromElement(reader);
                        break;
                    case "text":
                        dragBox.Text = await GetTextFromElement(reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Basic Question Functions
        static private async Task<Question> ReadBasicQuestion(XmlReader reader, int id)
        {
            Question question = new Question();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }
        #endregion
        #region Cloze Question Functions
        static private async Task<ClozeQuestion> ReadClozeQuestion(XmlReader reader, int id)
        {
            ClozeQuestion question = new ClozeQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                {
                    ParseClozeTextIntoSubquestions(question);
                    return question;

                }
                switch (reader.Name)
                {
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }
        static Regex clozeRegex = new Regex("{[\\s\\S]*?}");
        static StyleLength inputWidth = new StyleLength { value = new Length { value = 35, unit = LengthUnit.Percent } };
        internal static void ParseClozeTextIntoSubquestions(ClozeQuestion question)
        {
            string text = question.Questiontext.Text;
            int currentIndex = 0;
            foreach (Match clozeMatch in clozeRegex.Matches(text))
            {
                var clozeSubQuestion = ParseClozeSubquestion(clozeMatch.Value);
                //Text in Front of/Between Cloze Match
                if (currentIndex < clozeMatch.Index)
                {
                    clozeSubQuestion.PreText = text.Substring(currentIndex, clozeMatch.Index - currentIndex);
                }
                currentIndex = clozeMatch.Index + clozeMatch.Length;
                question.SubQuestions.Add(clozeSubQuestion);
            }
            question.PostText = text.Substring(currentIndex, text.Length - currentIndex);
        }
        private static ClozeSubQuestion ParseClozeSubquestion(string input)
        {

            // Remove the outer brackets
            if (input.StartsWith("{") && input.EndsWith("}"))
            {
                input = input.Substring(1, input.Length - 2);
            }

            // Split by sub-question delimiters (assuming there's only one sub-question for simplicity)
            var parts = input.Split(':', 3);
            var cloze = new ClozeSubQuestion();
            cloze.Grade = 1;
            // Parse grade (if any)
            if (int.TryParse(parts[0], out int grade))
            {
                cloze.Grade = grade;
            }

            // Parse question type

            parts[1] = parts[1].ToUpper();
            if (parts[1].Length == 2)
            {
                foreach (var pair in typeMapping)
                {
                    parts[1] = parts[1].Replace(pair.Key, pair.Value);
                }

            }
            else
            {
                foreach (var pair in typeMapping)
                {
                    parts[1] = parts[1].Replace(pair.Key, pair.Value + "_");
                }
            }
            if (Enum.TryParse(parts[1], out ClozeSubQuestion.ClozeType clozeType))
            {
                cloze.Type = clozeType;
            }

            // Parse answer options and feedback
            var answersAndFeedback = parts[2].Split('~');

            foreach (var part in answersAndFeedback)
            {
                var clozeAnswer = new Answer();
                var answerAndFeedback = part.Split("#");
                var answer = answerAndFeedback[0];
                if (answerAndFeedback.Length > 1)
                    clozeAnswer.Feedback = answerAndFeedback[1];
                if (answer.StartsWith("="))
                {
                    clozeAnswer.Fraction = 100;
                    clozeAnswer.Answertext = answer.Substring(1);
                }
                else if (answer.StartsWith("%"))
                {
                    var answerAndPercentage = answer.Split("%");
                    clozeAnswer.Fraction = int.Parse(answerAndPercentage[1]);
                    clozeAnswer.Answertext = answerAndPercentage[2];
                }
                else
                {
                    clozeAnswer.Fraction = 0;
                    clozeAnswer.Answertext = answer;
                }
                cloze.Answers.Add(clozeAnswer);
            }

            return cloze;
        }
        static Dictionary<string, string> typeMapping = new Dictionary<string, string> {
            {"SA", "SHORTANSWER" },
            {"NM", "NUMERICAL"},
            {"MC", "MULTICHOICE"},
            {"MR","MULTIRESPONSE"  }
        };
        #endregion

        #region Essay Question Functions
        static private async Task<EssayQuestion> ReadEssayQuestion(XmlReader reader, int id)
        {
            EssayQuestion question = new EssayQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "responseformat":
                        question.ResponseFormat = await GetTextFromElement(reader);
                        break;
                    case "responserequired":
                        question.ResponseRequired = await GetBoolFromElement(reader);
                        break;
                    case "responsefieldlines":
                        question.ResponseFieldLines = await GetIntFromElement(reader);
                        break;
                    case "minwordlimit":
                        question.MinWordLimit = await GetIntFromElement(reader);
                        break;
                    case "maxwordlimit":
                        question.MaxWordLimit = await GetIntFromElement(reader);
                        break;
                    case "attachments":
                        question.Attachments = await GetIntFromElement(reader);
                        break;
                    case "attachmentsrequired":
                        question.AttachmentsRequired = await GetBoolFromElement(reader);
                        break;
                    case "maxbytes":
                        question.MaxBytes = await GetIntFromElement(reader);
                        break;
                    case "filetypeslist":
                        question.FileTypesList = await GetTextFromElement(reader);
                        break;
                    case "graderinfo":
                        question.GraderInfo = await GetTextFromElement(reader, 2);
                        break;
                    case "responsetemplate":
                        question.ResponseTemplate = await GetTextFromElement(reader, 2);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Matching Functions
        static private async Task<MatchingQuestion> ReadMatchingQuestion(XmlReader reader, int id)
        {
            MatchingQuestion question = new MatchingQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "subquestion":
                        await ReadSubquestion(reader, question);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadSubquestion(XmlReader reader, MatchingQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            SubQuestion subQuestion = new SubQuestion();
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "subquestion")
                { question.SubQuestions.Add(subQuestion); return; }
                switch (reader.Name)
                {
                    case "answer":
                        subQuestion.Answer = await GetTextFromElement(reader, 2);
                        break;
                    case "text":
                        subQuestion.Text = await GetTextFromElement(reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Numerical Functions
        static private async Task<Question> ReadNumericalQuestion(XmlReader reader, int id)
        {
            NumericQuestion question = new NumericQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "answer":
                        await ReadNumericalAnswer(reader, question);
                        break;
                    case "units":
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckUnitQuestionNodes2(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadNumericalAnswer(XmlReader reader, NumericQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            NumericAnswer answer = new NumericAnswer();
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.Answers.Add(answer); return; }
                switch (reader.Name)
                {
                    case "tolerance":
                        answer.Tolerance = await GetFloatFromElement(reader);
                        break;
                    default:
                        await CheckBasicAnswerNodes(reader, answer);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Molecule Question Functions
        static private async Task<MolecularQuestion> ReadMolecularQuestion(XmlReader reader, int id)
        {
            MolecularQuestion question = new MolecularQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "usecase":
                        question.UseCase = await GetIntFromElement(reader);
                        break;
                    case "allowsubscript":
                        question.AllowSubscript = await GetBoolFromElement(reader);
                        break;
                    case "allowsuperscript":
                        question.AllowSuperscript = await GetBoolFromElement(reader);
                        break;
                    case "forcelength":
                        question.ForceLength = await GetBoolFromElement(reader);
                        break;
                    case "applydictionarycheck":
                        question.ApplyDictionaryCheck = await GetTextFromElement(reader);
                        break;
                    case "extenddictionary":
                        question.ExendDictionary = true;
                        await reader.ReadAsync();
                        break;
                    case "sentencedividers":
                        question.SentenceDividers = await GetTextFromElement(reader);
                        break;
                    case "converttospace":
                        question.ConvertToSpace = true;
                        await reader.ReadAsync();
                        break;
                    case "modelanswer":
                        question.ModelAnswer = true;
                        await reader.ReadAsync();
                        break;
                    case "answer":
                        await ReadMolecularAnswer(reader, question);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadMolecularAnswer(XmlReader reader, MolecularQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            MolecularAnswer answer = new MolecularAnswer();
            // we need a do-while loop here because the first information is stored in an attribute instead as a text inside the node like in every other node
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.Answers.Add(answer); return; }
                switch (reader.Name)
                {
                    case "atomcount":
                        answer.Atomcount = await GetIntFromElement(reader);
                        break;
                    default:
                        await CheckBasicAnswerNodes(reader, answer);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region RandomsMatch Functions
        static private async Task<RandomMatchingQuestion> ReadRandomsMatchQuestion(XmlReader reader, int id)
        {
            RandomMatchingQuestion question = new RandomMatchingQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "choose":
                        question.Choose = await GetIntFromElement(reader);
                        break;
                    case "subcats":
                        question.Subcategories = await GetBoolFromElement(reader);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        await CheckBaseMultipleQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region ShortAnswer Functions
        static private async Task<ShortQuestion> ReadShortQuestion(XmlReader reader, int id)
        {
            ShortQuestion question = new ShortQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "usecase":
                        question.UseCase = await GetIntFromElement(reader);
                        break;
                    case "answer":
                        await ReadShortQuestionAnswer(reader, question);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadShortQuestionAnswer(XmlReader reader, TrueFalseQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            MultipleChoiceAnswer answer = new MultipleChoiceAnswer();
            // we need a do-while loop here because the first information is stored in an attribute instead as a text inside the node like in every other node
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.Answers.Add(answer); return; }
                await CheckBasicAnswerNodes(reader, answer);
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Stack Question Functions
        static private async Task<StackQuestion> ReadStackQuestion(XmlReader reader, int id)
        {
            StackQuestion question = new StackQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "stackversion":
                        question.StackVersion = await GetTextFromElement(reader, 2);
                        break;
                    case "questionvariables":
                        question.QuestionVariables = await GetTextFromElement(reader, 2);
                        break;
                    case "specificfeedback":
                        question.SpecificFeedback = await GetTextFromElement(reader, 2);
                        break;
                    case "questionnote":
                        question.QuestionNote = await GetTextFromElement(reader, 2);
                        break;
                    case "questiondescription":
                        question.QuestionDescription = await GetTextFromElement(reader, 2);
                        break;
                    case "questionsimplify":
                        question.QuestionSimplify = await GetBoolFromElement(reader);
                        break;
                    case "assumepositive":
                        question.AssumePositive = await GetBoolFromElement(reader);
                        break;
                    case "assumereal":
                        question.AssumeReal = await GetBoolFromElement(reader);
                        break;
                    case "prtcorrect":
                        question.PRTCorrect = await GetTextFromElement(reader, 2);
                        break;
                    case "prtpartiallycorrect":
                        question.PRTPartiallyCorrect = await GetTextFromElement(reader, 2);
                        break;
                    case "prtincorrect":
                        question.PRTIncorrect = await GetTextFromElement(reader, 2);
                        break;
                    case "decimals":
                        question.Decimals = await GetTextFromElement(reader);
                        break;
                    case "multiplicationsign":
                        question.MultiplicationSign = await GetTextFromElement(reader);
                        break;
                    case "sqrtsign":
                        question.SquareRootSign = await GetIntFromElement(reader);
                        break;
                    case "complexno":
                        question.ComplexNumber = await GetTextFromElement(reader);
                        break;
                    case "inversetrig":
                        question.InverseTrigonomy = await GetTextFromElement(reader);
                        break;
                    case "logicsymbol":
                        question.LogicSymbol = await GetTextFromElement(reader);
                        break;
                    case "matrixparens":
                        question.MatrixParenthesis = await GetTextFromElement(reader);
                        break;
                    case "variantsselectionseed":
                        question.VariantsSelectionSeed = true;
                        break;
                    case "input":
                        break;
                    case "prt":
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadStackInput(XmlReader reader, StackQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            StackQuestionInput input = new StackQuestionInput();
            // we need a do-while loop here because the first information is stored in an attribute instead as a text inside the node like in every other node
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.Inputs.Add(input); return; }
                switch (reader.Name)
                {
                    case "name":
                        input.Name = await GetTextFromElement(reader);
                        break;
                    case "type":
                        input.Type = await GetTextFromElement(reader);
                        break;
                    case "tans":
                        input.Tans = await GetTextFromElement(reader);
                        break;
                    case "boxsize":
                        input.BoxSize = await GetIntFromElement(reader);
                        break;
                    case "strictsyntax":
                        input.StrictSyntax = await GetBoolFromElement(reader);
                        break;
                    case "insertstars":
                        input.InsertStars = await GetIntFromElement(reader);
                        break;
                    case "syntaxhint":
                        input.SyntaxHint = await GetTextFromElement(reader);
                        break;
                    case "syntaxattribute":
                        input.SyntaxAttribute = await GetIntFromElement(reader);
                        break;
                    case "forbidwords":
                        input.ForbidWords = await GetTextFromElement(reader);
                        break;
                    case "allowwords":
                        input.AllowWords = await GetTextFromElement(reader);
                        break;
                    case "forbidfloat":
                        input.ForbidFloat = await GetBoolFromElement(reader);
                        break;
                    case "requirelowestterms":
                        input.RequireLowestTerms = await GetBoolFromElement(reader);
                        break;
                    case "checkanswertype":
                        input.CheckAnswerType = await GetBoolFromElement(reader);
                        break;
                    case "mustverify":
                        input.MustVerify = await GetBoolFromElement(reader);
                        break;
                    case "showvalidation":
                        input.ShowValidation = await GetBoolFromElement(reader);
                        break;
                    case "options":
                        input.Options = await GetTextFromElement(reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadStackPRT(XmlReader reader, StackQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            StackQuestionPRT stackQuestionPRT = new StackQuestionPRT();
            // we need a do-while loop here because the first information is stored in an attribute instead as a text inside the node like in every other node
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.PRTs.Add(stackQuestionPRT); return; }
                switch (reader.Name)
                {
                    case "name":
                        stackQuestionPRT.Name = await GetTextFromElement(reader);
                        break;
                    case "value":
                        stackQuestionPRT.Value = await GetFloatFromElement(reader);
                        break;
                    case "autosimplify":
                        stackQuestionPRT.AutoSimplify = await GetBoolFromElement(reader);
                        break;
                    case "feedbackstyle":
                        stackQuestionPRT.FeedbackStyle = await GetIntFromElement(reader);
                        break;
                    case "feedbackvariables":
                        stackQuestionPRT.FeedbackVariables = await GetTextFromElement(reader);
                        break;
                    case "node":
                        await ReadprtNode(reader, stackQuestionPRT);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }

        static private async Task ReadprtNode(XmlReader reader, StackQuestionPRT stackQuestionPRT)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            PRTNode prtNode = new PRTNode();
            // we need a do-while loop here because the first information is stored in an attribute instead as a text inside the node like in every other node
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { stackQuestionPRT.Nodes.Add(prtNode); return; }
                switch (reader.Name)
                {
                    case "name":
                        prtNode.Name = await GetTextFromElement(reader);
                        break;
                    case "description":
                        prtNode.Description = await GetTextFromElement(reader);
                        break;
                    case "answertest":
                        prtNode.AnswerTest = await GetTextFromElement(reader);
                        break;
                    case "sans":
                        prtNode.Sans = await GetTextFromElement(reader);
                        break;
                    case "tans":
                        prtNode.Tans = await GetTextFromElement(reader);
                        break;
                    case "testoptions":
                        prtNode.TestOptions = await GetTextFromElement(reader);
                        break;
                    case "quiet":
                        prtNode.Quiet = await GetIntFromElement(reader);
                        break;
                    case "truescoremode":
                        prtNode.TrueScoreMode = await GetTextFromElement(reader);
                        break;
                    case "truescore":
                        prtNode.TrueScore = await GetIntFromElement(reader);
                        break;
                    case "truepenalty":
                        prtNode.TruePenalty = await GetIntFromElement(reader);
                        break;
                    case "truenextnode":
                        prtNode.TrueNextNode = await GetIntFromElement(reader);
                        break;
                    case "trueanswernote":
                        prtNode.TrueAnswerNote = await GetTextFromElement(reader);
                        break;
                    case "truefeedback":
                        prtNode.TrueFeedback = await GetTextFromElement(reader);
                        break;
                    case "falsescoremode":
                        prtNode.FalseScoreMode = await GetTextFromElement(reader);
                        break;
                    case "falsescore":
                        prtNode.FalseScore = await GetIntFromElement(reader);
                        break;
                    case "falsepenalty":
                        prtNode.FalsePenalty = await GetIntFromElement(reader);
                        break;
                    case "falsenextnode":
                        prtNode.FalseNextNode = await GetIntFromElement(reader);
                        break;
                    case "falseanswernote":
                        prtNode.FalseAnswerNote = await GetTextFromElement(reader);
                        break;
                    case "falsefeedback":
                        prtNode.FalseFeedback = await GetTextFromElement(reader);
                        break;
                }
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region TrueFalse Question Functions
        static private async Task<TrueFalseQuestion> ReadTrueFalseQuestion(XmlReader reader, int id)
        {
            TrueFalseQuestion question = new TrueFalseQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "answer":
                        await ReadShortQuestionAnswer(reader, question);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Ordering Question Functions
        static private async Task<OrderingQuestion> ReadOrderingQuestion(XmlReader reader, int id)
        {
            OrderingQuestion question = new OrderingQuestion();
            question.QuestionID = id;

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "question")
                    return question;
                switch (reader.Name)
                {
                    case "answer":
                        await ReadOrderingQuestionAnswer(reader, question);
                        break;
                    case "numberingstyle":
                        question.NumberingStyle = await GetTextFromElement(reader);
                        break;
                    case "correctfeedback":
                        question.CorrectFeedback = await GetTextFromElement(reader, 2);
                        break;
                    case "partiallycorrectfeedback":
                        question.PartiallyCorrectFeedback = await GetTextFromElement(reader, 2);
                        break;
                    case "incorrectfeedback":
                        question.IncorrectFeedback = await GetTextFromElement(reader, 2);
                        break;
                    case "shownumcorrect":
                        question.ShowNumCorrect = await GetBoolFromElement(reader);
                        break;
                    case "layouttype":
                        question.LayoutType = await GetTextFromElement(reader);
                        break;
                    case "selecttype":
                        question.SelectType = await GetTextFromElement(reader);
                        break;
                    case "selectcount":
                        question.SelectCount = await GetIntFromElement(reader);
                        break;
                    case "gradingtype":
                        question.GradingType = await GetTextFromElement(reader);
                        break;
                    case "showgrading":
                        question.ShowGrading = await GetTextFromElement(reader);
                        break;
                    default:
                        await CheckBaseQuestionNodes(reader, question);
                        break;
                }
            }
            throw new FormatException("Question wrongly formatted");
        }
        static private async Task ReadOrderingQuestionAnswer(XmlReader reader, OrderingQuestion question)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            OrderingAnswer answer = new OrderingAnswer();
            // we need a do-while loop here because the first information is stored in an attribute instead as a text inside the node like in every other node
            do
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "answer")
                { question.Answers.Add(answer); return; }
                switch (reader.Name)
                {
                    case "answer":
                        if (!reader.HasAttributes || reader.GetAttribute("format") == null)
                            throw new FormatException("Answer Node has no Format");
                        answer.Format = reader["format"];
                        break;
                }
                await CheckBasicAnswerNodes(reader, answer);
            } while (await reader.ReadAsync());
            throw new FormatException("Question wrongly formatted");
        }
        #endregion

        #region Helper Functions
        static private async Task<Shape> GetShapeFromElement(string shapeString, XmlReader reader, int readingDepth = 1, bool allowEmptyText = false, bool removeHTMLTags = true)
        {
            string[] coordsArray = (await GetTextFromElement(reader, readingDepth, allowEmptyText, removeHTMLTags)).Split(";");
            switch (shapeString)
            {
                case "rectangle":
                    if (coordsArray.Length != 2)
                        throw new FormatException($"coords Element is malformed: {String.Join(',', coordsArray)}");
                    Rectangle rectangle = new Rectangle();
                    IEnumerable<int> topLeft = coordsArray[0].Split(",").Select(coord => int.Parse(coord));
                    rectangle.TopLeft = new Point(topLeft.ElementAt(0), topLeft.ElementAt(1));
                    IEnumerable<int> lengths = coordsArray[1].Split(",").Select(coord => int.Parse(coord));
                    rectangle.Width = lengths.ElementAt(0);
                    rectangle.Height = lengths.ElementAt(1);
                    return rectangle;
                case "circle":
                    if (coordsArray.Length != 2)
                        throw new FormatException($"coords Element is malformed: {String.Join(',', coordsArray)}");
                    Circle circle = new Circle();
                    IEnumerable<int> center = coordsArray[0].Split(",").Select(coord => int.Parse(coord));
                    circle.Center = new Point(center.ElementAt(0), center.ElementAt(1));
                    circle.Radius = int.Parse(coordsArray[1]);
                    return circle;
                case "polygon":
                    if (coordsArray.Length < 3)
                        throw new FormatException($"coords Element is malformed: {String.Join(',', coordsArray)}");
                    Polygon polygon = new Polygon();
                    foreach (string coord in coordsArray)
                    {
                        IEnumerable<int> point = coordsArray[0].Split(",").Select(coord => int.Parse(coord));
                        polygon.Points.Add(new Point(point.ElementAt(0), point.ElementAt(1)));
                    }
                    return polygon;
                default:
                    throw new Exception($"Unkown shape encountered {shapeString}");
            }


        }

        static private async Task<bool> GetBoolFromElement(XmlReader reader, int readingDepth = 1, bool allowEmptyText = false, bool removeHTMLTags = true)
        {
            string text = await GetTextFromElement(reader, readingDepth, allowEmptyText, removeHTMLTags);
            return text.ToLower() == "true" || text == "1";
        }

        static private async Task<MoodleFile> GetMoodleFileFromElement(XmlReader reader, int readingDepth = 1, bool allowEmptyText = false, bool removeHTMLTags = true)
        {
            MoodleFile result = new MoodleFile();
            var name = reader.GetAttribute("name");

            for (int i = 0; i < name.Length; i++)
            {
                char curChar = name[i];
                var charValue = Convert.ToInt32(curChar);
                if (Char.IsLetterOrDigit(curChar) || curChar == '.' || curChar == '-')
                {
                    result.Name += curChar;
                }
                else
                {
                    try
                    {
                        result.Name += Uri.HexEscape(curChar);
                    }
                    catch (Exception)
                    {
                        result.Name += Uri.EscapeUriString(curChar.ToString());
                    }

                }
            }
            result.Encoding = reader.GetAttribute("encoding");
            result.Data = Convert.FromBase64String(await GetTextFromElement(reader, readingDepth, allowEmptyText, removeHTMLTags));
            return result;
        }

        static private async Task<int> GetIntFromElement(XmlReader reader, int readingDepth = 1, bool allowEmptyText = false, bool removeHTMLTags = true)
        {
            return int.Parse(await GetTextFromElement(reader, readingDepth, allowEmptyText, removeHTMLTags));
        }

        static private async Task<float> GetFloatFromElement(XmlReader reader, int readingDepth = 1, bool allowEmptyText = false, bool removeHTMLTags = true)
        {
            return float.Parse(await GetTextFromElement(reader, readingDepth, allowEmptyText, removeHTMLTags), CultureInfo.InvariantCulture);
        }

        static private async Task<Questiontext> GetQuestionText(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            Questiontext questiontext = new Questiontext();
            await reader.ReadAsync();
            questiontext.Text = await GetTextFromElement(reader);
            await reader.ReadAsync();
            while (reader.Name != "questiontext")
            {
                questiontext.Files.Add(await GetMoodleFileFromElement(reader));
                await reader.ReadAsync();
            }
            return questiontext;
        }

        static private async Task<Questiontext> GetTextWithImages(XmlReader reader, string endTag)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            Questiontext questiontext = new Questiontext();
            await reader.ReadAsync();
            questiontext.Text = await GetTextFromElement(reader);
            await reader.ReadAsync();
            while (reader.Name != endTag)
            {
                questiontext.Files.Add(await GetMoodleFileFromElement(reader));
                await reader.ReadAsync();
            }
            return questiontext;
        }

        static private async Task<String> GetTextFromElement(XmlReader reader, int readingDepth = 1, bool allowEmptyText = true, bool removeHTMLTags = true)
        {
            if (reader == null)
                throw new ArgumentNullException("Reader should not be null");
            if (readingDepth == 1 && reader.IsEmptyElement)
                return "";
            string debugInfo = reader.Name;
            string result = "";
            // Traverse the nodes until we are deep enough
            for (int i = 0; i < readingDepth; i++)
            {
                reader.ReadAsync().Wait();
                // Handle special <text /> case inside another node
                if (reader.IsEmptyElement)
                {
                    for (int j = 0; j < i + 1; j++)
                    {
                        reader.ReadAsync().Wait();
                    }
                    return "";
                }
            }
            // Sanity check if there is actually Text
            if (!allowEmptyText && reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.CDATA)
                throw new FormatException($"Did not found text at the given depth of {readingDepth} reader is currently at line {((IXmlLineInfo)reader).LineNumber} and LinePosition: {((IXmlLineInfo)reader).LinePosition}");
            if (allowEmptyText && reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.CDATA)
                readingDepth -= 2;
            result = await reader.GetValueAsync();
            // Traverse all closing Nodes
            for (int i = 0; i < readingDepth; i++)
            {
                reader.ReadAsync().Wait();
                if (reader.NodeType != XmlNodeType.EndElement)
                    throw new FormatException($"Found a non-closing Element unexpectedly at the given depth of {readingDepth} reader is currently at line {((IXmlLineInfo)reader).LineNumber} and LinePosition: {((IXmlLineInfo)reader).LinePosition}");
            }
            if (removeHTMLTags)
            {
                //remove all html tags (like <p> and <span> except <img>)
                result = Regex.Replace(result, "<(?!img).*?>", String.Empty);
            }
            result = System.Net.WebUtility.HtmlDecode(result);
            return result;
        }

        #endregion
    }
}