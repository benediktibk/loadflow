namespace Database
{
    public interface IConnectionNetElements
    {
        void Add(Node node, PowerNet powerNet);
        void Add(FeedIn feedIn, PowerNet powerNet);
        void Add(Generator generator, PowerNet powerNet);
        void Add(Load load, PowerNet powerNet);
        void Add(Line line, PowerNet powerNet);
        void Add(Transformer transformer, PowerNet powerNet);

        void Update(Node node);
        void Update(FeedIn feedIn);
        void Update(Generator generator);
        void Update(Load load);
        void Update(Line line);
        void Update(Transformer transformer);

        void Remove(Node node);
        void Remove(FeedIn feedIn);
        void Remove(Generator generator);
        void Remove(Load load);
        void Remove(Line line);
        void Remove(Transformer transformer);
    }
}
