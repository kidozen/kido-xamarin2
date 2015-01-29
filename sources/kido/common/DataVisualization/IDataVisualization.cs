public interface IDataVisualization {
	void LaunchView (string visualization, string targetdirectory); 
	string GetDestinationDirectory (string visualization); 
	string GetTargetDirectory (); 
	bool UnzipFiles (string path, string zipname);
}