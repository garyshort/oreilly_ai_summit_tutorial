using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.ie.crf;
using System.IO;

namespace NerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Where do the classifiers live?
            string classifierDir = @"C:\Users\gashort\Documents\Presentations\AISummitTutorial\Lab1-NER-Start\stanford-ner-2017-06-09\classifiers\";

            // Select the classifier we want
            string selectedClassifier = "english.all.3class.distsim.crf.ser.gz";

            // Build the path to the selected classifier
            string classifierPath = classifierDir + selectedClassifier;

            // Load the selected classifier
            CRFClassifier classifier =
                CRFClassifier.getClassifierNoExceptions(classifierPath);

            // Load some text
            string text = File.ReadAllText(@"C:\Users\gashort\Documents\Presentations\AISummitTutorial\Lab1-NER-Start\Demo\TaleOfTwoCities.txt");
            
            // Classify the text
            string classified = classifier.classifyToString(text);
            File.WriteAllText(
                @"C:\Users\gashort\Documents\Presentations\AISummitTutorial\Lab1-NER-Start\ModelOutput\out.txt", 
                classified);
        }
    }
}
