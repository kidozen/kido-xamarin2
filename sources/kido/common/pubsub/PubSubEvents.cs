public class ClockEventArgs : EventArgs {
    public int hour;
    public int minute;
    public int second;

    public ClockEventArgs(int hour, int minute, int second) {
        this.hour = hour;
        this.minute = minute;
        this.second = second;
    }
}

