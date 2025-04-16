# DSIronPython3
unoffical ironpython3 engine package/extension for dynamo leveraging dynamic python engine loading in Dynamo 2.18+

This dynamo package contains the ironPython3 engine and stdlib.

⚠️⚠️ This is not an Autodesk offically supported package! Please report bugs to this repo/forum only. ⚠️⚠️

Pull requsts will be reviewed when possible.

### How to publish a new version
We currently can't use the release PR option when making releases on mirrored repositories. There is a propsed follow-up improvment filed that would allow CILibrary to create a PR against the public repository instead (DYN-8724). But until that is done, our release process will be:

- Create a release branch on the internal repository
- Build the branch to publish
- Manually create a PR on the public repository with the changes introduced by the release branch (the updated version number in the pipeline file). 
- Review and merge this PR
- Delete the release branch on the internal repository