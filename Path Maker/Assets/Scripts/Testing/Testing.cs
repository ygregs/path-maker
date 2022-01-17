using UnityEngine;

public class Testing : MonoBehaviour
{
    private delegate void TestDelegateOne();
    private delegate bool TestDelegateTwo(int i);

    private TestDelegateOne myTestDelegateOne;
    private TestDelegateTwo myTestDelegateTwo;

    private void Start()
    {
        myTestDelegateOne += TestFunctionOne;
        myTestDelegateOne += TestFunctionTwo;

        //myTestDelegateOne();

        myTestDelegateOne -= TestFunctionTwo;

        //myTestDelegateOne();

        myTestDelegateTwo += TestIFunction;

        print(myTestDelegateTwo(10));
    }

    private void TestFunctionOne()
    {
        print("TestFunctionOne");
    }

    private void TestFunctionTwo()
    {
        print("TestFunctionTwo");
    }

    private bool TestIFunction(int x)
    {
        return x > 5;
    }
}