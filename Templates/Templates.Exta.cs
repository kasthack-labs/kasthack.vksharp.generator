namespace kasthack.vksharp.generator.Templates
{
    partial class Methods
    {

        public Methods(MethodsSchema methodsSchema, ResponsesSchema responsesSchema, ResponsesSchema objectsSchema)
        {
            M = methodsSchema;
            R = responsesSchema;
            O = objectsSchema;
        }

        public MethodsSchema M { get; }
        public ResponsesSchema R { get; }
        public ResponsesSchema O { get; }
    }
    partial class Objects
    {
        public Objects(ResponsesSchema objectsSchema) => O = objectsSchema;

        public ResponsesSchema O { get; }
    }
    partial class Responses
    {

        public Responses(ResponsesSchema responsesSchema, ResponsesSchema objectsSchema)
        {
            R = responsesSchema;
            O = objectsSchema;
        }

        public ResponsesSchema R { get; }
        public ResponsesSchema O { get; }
    }
}
