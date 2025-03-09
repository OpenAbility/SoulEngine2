using SoulEngine.SequenceScript.Machine;

namespace SoulEngine.SequenceScript.Emitter;

public class OpWriter
{
    private List<Template> templates = new List<Template>();


    public Instruction[] Build()
    {
        Instruction[] buffer = new Instruction[templates.Count];

        for (int i = 0; i < templates.Count; i++)
        {
            if(templates[i].Label != null)
                buffer[i] = new Instruction(templates[i].OpCode, templates[i].DynValue);
            else
            {
                if (templates[i].Label!.Location == -1)
                    throw new Exception("Label never resolved!");
                buffer[i] = new Instruction(templates[i].OpCode, new DynValue(templates[i].Label!.Location));
            }
                
        }

        return buffer;
    }
    
    public OpWriter Instruction(OpCode code, DynValue value)
    {
        templates.Add(new Template()
        {
            OpCode = code,
            DynValue = value
        });
        return this;
    }
    
    public OpWriter Instruction(OpCode code, out Label label)
    {
        label = new Label();
        
        templates.Add(new Template()
        {
            OpCode = code,
            Label = label
        });
        return this;
    }
    
    public OpWriter Instruction(OpCode code, Label label)
    {
        templates.Add(new Template()
        {
            OpCode = code,
            Label = label
        });
        return this;
    }

    public OpWriter Label(Label label)
    {
        label.Location = templates.Count;
        return this;
    }
    
    public OpWriter Label(out Label label)
    {
        label = new Label();
        label.Location = templates.Count;
        return this;
    }
    

    private struct Template
    {
        public OpCode OpCode;
        public DynValue DynValue;
        public Label? Label;
    }
}