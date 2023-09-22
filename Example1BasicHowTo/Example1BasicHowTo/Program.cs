using RecordLinkage.Core;
using RecordLinkageNet.Core;
using RecordLinkageNet.Core.Compare;
using RecordLinkageNet.Core.Data.Transpose;

//we genrate a hand test list
List<TestDataPerson> testDataPeopleA = new List<TestDataPerson>
{
    new TestDataPerson("Thomas", "Mueller", "Lindenstrasse", "Testhausen", "012345")
};
var tabA = TableConverter.CreateTableFeatherFromDataObjectList(testDataPeopleA);

List<TestDataPerson> testDataPeopleB = new List<TestDataPerson>
{
    new TestDataPerson("Thomas", "Mueller", "Lindetrasse", "Testhausen", "12345"),
    new TestDataPerson("Thomas", "Mueller", "Lindenstrasse", "Testcity", "012345"),
    new TestDataPerson("Thomas", "Müller", "Lindenstrasse", "Testcity", "012345"),
    new TestDataPerson("Tomas", "Müller", "Lindenstroad", "Testhausen", "012342"),
    new TestDataPerson("Tomas", "Müller", "Lindenstroad", "Dorf", "012342")
};
var tabB = TableConverter.CreateTableFeatherFromDataObjectList(testDataPeopleB);

//build a simle configuration
ConditionList conList = new ConditionList();
//setup with string similarity what should be compared
conList.String("NameFirst", "NameFirst", Condition.StringMethod.JaroWinklerSimilarity);
conList.String("Street", "Street", Condition.StringMethod.JaroWinklerSimilarity);
//set this to exact, because we want to compare the postal code exact
conList.String("PostalCode", "PostalCode", Condition.StringMethod.Exact);
conList.String("NameLast", "NameLast", Condition.StringMethod.JaroWinklerSimilarity);

//add weights to specify similarity
Dictionary<string, float> scoreTable = new Dictionary<string, float>
{
    { "NameLast", 2.0f },
    { "NameFirst", 1.5f },
    { "Street", 0.9f },
    { "PostalCode", 1f },
};

//add weight
foreach (Condition c in conList)
{
    c.ScoreWeight = scoreTable[c.NameColNewLabel];
}

//do configuration
Configuration.Instance.Reset();
Configuration config = Configuration.Instance;
config.AddIndex(new IndexFeather().Create(tabA, tabB));
config.AddConditionList(conList);
config.SetStrategy(Configuration.CalculationStrategy.WeightedConditionSum);
config.SetNumberTransposeModus(NumberTransposeHelper.TransposeModus.LOG10);

//we init a worker
WorkScheduler workScheduler = new WorkScheduler();
//init add add a cancel token to the worker, for canceling the pipeline
var pipeLineCancellation = new CancellationTokenSource();
var resultTask = workScheduler.Compare(pipeLineCancellation.Token);

//we wait for the result
await resultTask;

//write the amountResults to console
Console.WriteLine($"Amount of results: {resultTask.Result.Count()}");

//filter the result with a min score
FilterRelativMinScore filter = new FilterRelativMinScore(0.7f);
MatchCandidateList filteredList = filter.Apply(resultTask.Result);

//print the filtered list with iterate and score
foreach (MatchCandidate match in filteredList)
{
    Console.WriteLine("##########");
    IndexPair idxPair = match.GetIndexPair();
    Console.WriteLine($"Candidate: IndexA {idxPair.aIdx} - IndexB {idxPair.bIdx}");
    Console.WriteLine($"Candidate Score: {match.GetScore().GetScoreValue()}");
    //print the data candidates
    Console.WriteLine($"Candidate A: {testDataPeopleA[(int)idxPair.aIdx]}");
    Console.WriteLine($"Candidate B:{testDataPeopleB[(int)idxPair.bIdx]}");
}

//wait for key press
Console.ReadKey();