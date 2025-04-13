using SoulEngine.SequenceScript.Machine;
using ValueType = SoulEngine.SequenceScript.Machine.ValueType;

namespace SoulEngine.SequenceScript.Emitter;

public class OpWriter
{
    private List<Template> templates = new List<Template>();


    public Instruction[] Build()
    {
        Instruction[] buffer = new Instruction[templates.Count];

        for (int i = 0; i < templates.Count; i++)
        {
            Template template = templates[i];
            
            if(template.Label == null)
                buffer[i] = new Instruction(template.OpCode, template.DynValue);
            else
            {
                if (template.Label!.Location == -1)
                    throw new Exception("Label never resolved!");
                buffer[i] = new Instruction(template.OpCode, new DynValue(template.Label.Location));
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

    
    public OpWriter Instruction(OpCode code)
    {
        templates.Add(new Template()
        {
            OpCode = code,
            DynValue = new DynValue(ValueType.Bogus, null!)
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
        public bool LabelDefinition;
    }
}