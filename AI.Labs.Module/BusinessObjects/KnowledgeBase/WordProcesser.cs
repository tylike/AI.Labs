using System.Diagnostics;

namespace AI.Labs.Module.BusinessObjects.KnowledgeBase
{
    public class WordProcesser
    {
        static StanfordCoreNLP pipeline;
        

        public async static Task Load()
        {

            //在您提供的代码段中，您正在使用Stanford CoreNLP库来处理文本。根据代码，您设置了属性只使用tokenize注释器。
            //然而，您的代码中的注释部分提到了其他的注释器，比如ssplit(句子分割), pos(词性标注), lemma(词元化), ner(命名实体识别), parse(句法分析), dcoref(指代消解),
            //和 sentiment(情感分析)。这些注释器已被注释掉了，所以不会在pipeline中应用。
            //正常情况下，如果您的pipeline正确地设置了tokenize和ssplit注释器，word变量应该输出单个词，而不是整个句子。如果word输出的是整个句子，那么可能存在以下问题之一：
            //tokenize注释器没有正确运行，因此没有正确生成tokens。
            //在将tokens添加到CoreMap对象时出现了问题，导致每个token实际上是整个句子。
            //代码中的word变量获取的可能不是TextAnnotation，而是与整个句子相关的另一个注释。
            //为了确保您从CoreLabel对象中获取的确实是单个词，您需要检查word变量的确切获取方式。在您的代码中，word变量的赋值语句应该类似于下面的代码：
            //var word = token.get(typeof(CoreAnnotations.TextAnnotation));
            //确保CoreAnnotations.TextAnnotation类确实是用来获取单个token文本的注释。如果您在Java中开发，那么您的代码应该看起来像这样：
            //String word = token.get(CoreAnnotations.TextAnnotation.class);
            //如果您还是遇到问题，请检查以下可能的问题：
            //确保text变量（ViewCurrentObject.Text）中的文本是正确的，并且确实包含了您想要分析的文本。
            //确保您的环境（classpath等）正确设置，Stanford CoreNLP库可以找到并加载所有必要的模型。
            //如果问题依然存在，可能需要检查Stanford CoreNLP库的版本，并确保没有bug或兼容性问题。
            //如果您能提供更多的错误输出或日志信息，或者具体的例子，我可能能更具体地帮助您解决问题。
            await Task.Run(() =>
            {
                var props = new Properties();
                //props.setProperty("annotators", "tokenize, ssplit");//, pos, lemma, ner, parse, dcoref, sentiment
                var basePath = "f:/models/corenlp/";
                //props.setProperty("tokenize.language", "zh");
                //props.setProperty("segment.model", basePath+"edu/stanford/nlp/models/segmenter/chinese/ctb.gz");
                //props.setProperty("pos.model", basePath + "edu/stanford/nlp/models/pos-tagger/chinese-distsim.tagger");
                //props.setProperty("ner.model", basePath + "edu/stanford/nlp/models/ner/chinese.misc.distsim.crf.ser.gz");
                //props.setProperty("parse.model", basePath + "edu/stanford/nlp/models/lexparser/chinesePCFG.ser.gz");
                //props.setProperty("regexner.mapping", basePath + "edu/stanford/nlp/models/kbp/english/gazetteers/regexner_caseless.tab");
                //props.setProperty("ner.applyNumericClassifiers", "false");
                //props.setProperty("ner.useSUTime", "false");
                String propsFilePath = "f:/models/corenlp/StanfordCoreNLP-chinese.properties";
                // 使用Properties类加载配置文件
                props.load(new FileInputStream(propsFilePath));

                var curDir = Environment.CurrentDirectory;
                Directory.SetCurrentDirectory(basePath);
                pipeline = new StanfordCoreNLP(props);
                Directory.SetCurrentDirectory(curDir);
                Debug.WriteLine("加载完成！");
            });
        }


        public static String[] GetSentences(string text)
        {
            var document = new Annotation(text);
            pipeline.annotate(document);
            // 获取句子并遍历它们。
            var sentences = document.get(typeof(CoreAnnotations.SentencesAnnotation));
            if (sentences == null) { return null; } // 退出如果没有句子
            var sw = Stopwatch.StartNew();
            var rst = new List<string>();
            foreach (CoreMap sentence in sentences as ArrayList)
            {
                // 从句子中获取tokens。
                //foreach (CoreLabel token in sentence.get(typeof(CoreAnnotations.TokensAnnotation)) as ArrayList)
                //{
                //    //var word = token.get(typeof(CoreAnnotations.TextAnnotation));
                //    //Debug.WriteLine(word);
                //    //rst.Add(word.ToString());

                //    string ner = token.get(typeof(CoreAnnotations.NamedEntityTagAnnotation)) as string;
                //    rst.Add(ner);

                //}
                sentence.get(typeof(CoreAnnotations.SentencesAnnotation));
            }
            sw.Stop();
            return rst.ToArray();

        }

        public static string[] GetWords(string sentenceText)
        {
            var text = sentenceText;
            var document = new Annotation(text);
            pipeline.annotate(document);
            // 获取句子并遍历它们。
            var sentences = document.get(typeof(CoreAnnotations.SentencesAnnotation));
            if (sentences == null) { return null; } // 退出如果没有句子
            var sw = Stopwatch.StartNew();
            var rst = new List<string>();
            foreach (CoreMap sentence in sentences as ArrayList)
            {
                // 从句子中获取tokens。
                foreach (CoreLabel token in sentence.get(typeof(CoreAnnotations.TokensAnnotation)) as ArrayList)
                {
                    //var word = token.get(typeof(CoreAnnotations.TextAnnotation));
                    //Debug.WriteLine(word);
                    //rst.Add(word.ToString());

                    string ner = token.get(typeof(CoreAnnotations.NamedEntityTagAnnotation)) as string;
                    rst.Add(ner);

                }
            }
            sw.Stop();
            return rst.ToArray();
        }
    }
}
