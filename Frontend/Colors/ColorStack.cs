/*
This data structure is a stack of colors.
Its never empty,because it always has the black color at the bottom.
*/
namespace Frontend;

class ColorStack{
    //The stack behaviour is emulated using a list.
    List<Color> stack = new List<Color>();
    public ColorStack(){
        //Set BLACK as the default color to draw.
        stack.Add(Color.BLACK);
    }
    //Retrieve the color at the top of the stack.
    public Color Top {get => stack.Last(); }
    //Put the given color at the top of the stack.
    public void Push(Color color) => stack.Add(color);
    //Remove the color at the top of the stack. But always leave at least the black on the stack.
    public void Pop(){
        if(stack.Count > 1)stack.RemoveAt(stack.Count - 1);
    }
}