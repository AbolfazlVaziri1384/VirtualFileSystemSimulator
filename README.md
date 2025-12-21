# VirtualFileSystemSimulator

## Overview

VirtualFileSystemSimulator is a Windows Forms application built in C# that simulates a Unix-like virtual file system (VFS). It allows users to create, manage, and interact with a hierarchical file system in memory, persisted via JSON files. The simulator supports user authentication, permissions (similar to Unix chmod), groups, commits for version control, and a wide range of shell-like commands.

This project is ideal for educational purposes, demonstrating file system concepts, user management, and command-line interactions in a graphical interface.

### Key Features
- **User Registration & Login**: Secure authentication with password hashing
- **Virtual File System**: Full directory and file management with hierarchical structure
- **Unix-like Permissions**: Support for owner/group/others with read/write/execute flags
- **Group Management**: Share file systems between users with different access levels
- **Commit-based Version Control**: Create, revert, and manage file system snapshots
- **Tree View Visualization**: Graphical representation of the file system structure
- **Command-line Interface**: Familiar Unix-like commands with auto-completion
- **Symbolic & Hard Links**: Support for file and directory linking
- **Search Functionality**: Find files and directories by name or type
- **Comprehensive Error Handling**: Detailed error messages for all operations

The file system is stored in JSON files under the `VFS_JsonFiles` directory, with separate files for users and each commit version. Each user has their own "system file" (personal VFS) that can be shared with others through group permissions.

## Installation

### Prerequisites
- **.NET Framework** (version 4.7.2 or higher) or **.NET Core 6.0+**
- **Visual Studio 2019+** or any C# compiler for building the project
- **Windows OS** (designed for Windows Forms)

### Setup Steps
1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/VirtualFileSystemSimulator.git
   cd VirtualFileSystemSimulator
   ```

2. **Open in Visual Studio**:
   - Open `VirtualFileSystemSimulatorWinForm.sln` in Visual Studio
   - Restore NuGet packages if prompted

3. **Build the project**:
   - Press `Ctrl+Shift+B` or use Build → Build Solution
   - Ensure no compilation errors

4. **Run the application**:
   - Press `F5` to start debugging
   - The login form will appear automatically

**Note**: An "admin" user is created by default with password "admin" on first run.

## Getting Started

### First Launch
1. **Registration**:
   - If you're a new user, click "Register" on the login form
   - Enter a username and password (must match confirmation)
   - Upon success, you'll be prompted to log in

2. **Login**:
   - Enter your credentials
   - Upon successful login, the main interface opens with:
     - Command line interface (CLI) at the bottom
     - Command history panel on the left
     - Tree view visualization on the right
     - Current directory path displayed

3. **Default Environment**:
   - You start at the root directory (`/`)
   - Your personal system file is loaded (named after your username)
   - Current commit is "main"
   - You have Admin permissions in your own file system


**Components**:
- **Tree View**: Visual hierarchy of files and directories (blue = directories, red = files, violet = links)
- **Command History**: Shows executed commands and their outputs
- **Current Path**: Displays your current working directory
- **Command Line**: Enter commands here (press Enter to execute)

**Keyboard Shortcuts**:
- `Enter`: Execute command
- `Up/Down Arrow`: Navigate command history
- `Shift`: Auto-complete current command

## User Management & Permissions

### User Types
The system supports four user types, similar to Unix permissions:

| Type | Value | Description |
|------|-------|-------------|
| **Owner** | 0 | The creator/owner of a file/directory |
| **Group** | 1 | Users belonging to the same group |
| **Others** | 2 | All other users |
| **Admin** | 3 | System administrators with full access |

### Permission System
Permissions are represented as 9 characters: `rwxrwxrwx`
- Positions 0-2: Owner permissions (r/w/x)
- Positions 3-5: Group permissions (r/w/x)
- Positions 6-8: Others permissions (r/w/x)
- `r` = read, `w` = write, `x` = execute, `-` = no permission

**Default Permissions**:
- Directories: `rwxr-xr-x` (755)
- Files: `rw-r--r--` (644)

### Group Management
Users can share their file systems by adding others to their groups:
- Each user maintains a list of groups they belong to
- Group format: `username,permission_type` (e.g., `alice,1` for Group access)
- Multiple groups separated by `/`

## Version Control System

The simulator includes a commit-based version control system:

### Key Concepts
- **System File**: Each user's VFS (named after username)
- **Commit**: Snapshot of the VFS at a point in time
- **Default Branch**: "main" (created automatically)

### Commit Operations
- **Create**: Make a new snapshot of current state
- **Revert**: Load a previous commit (doesn't delete current)
- **List**: View all commits for a system file
- **Delete**: Remove a commit (cannot delete current)

### Storage
Commits are stored as JSON files:
- `vfs_username_commitname.json`
- Located in `VFS_JsonFiles/` directory
- Each commit is a complete copy of the file system

## Command Reference

All commands are entered in the CLI textbox. Commands are **case-sensitive**. Paths can be absolute (starting with `/`) or relative.

### File & Directory Operations

#### 1. `mkdir` - Create Directory
**Syntax**: `mkdir [-p] <directory_path>`
- Creates a new directory
- `-p`: Create parent directories if they don't exist
- **Requires**: Write permission in parent directory

**Examples**:
```bash
mkdir documents                    # Create 'documents' in current directory
mkdir /user/docs/projects          # Absolute path
mkdir -p /a/b/c                    # Create nested directories
mkdir ../sibling                   # Relative to parent
```

**Output**: `Directory 'documents' created successfully.`

---

#### 2. `touch` - Create or Update File
**Syntax**: `touch [-t "yyyy-MM-dd HH:mm"] <file_path>`
- Creates a new file or updates timestamp
- `-t`: Set custom timestamp
- **Requires**: Write permission in parent directory

**Examples**:
```bash
touch notes.txt                    # Create empty file
touch -t "2024-01-01 12:00" old.txt
touch ./docs/readme.md             # Relative path
```

**Output**: `File 'notes.txt' created successfully in 'documents'.`

---

#### 3. `ls` - List Directory Contents
**Syntax**: `ls [-l] [-a] [path]`
- Lists files and directories
- `-l`: Detailed view (permissions, owner, size, timestamp)
- `-a`: Show hidden files (starting with `.`)
- **Requires**: Read permission

**Examples**:
```bash
ls                                # List current directory
ls -l                             # Detailed list
ls -a                             # Include hidden files
ls -l -a /user/docs                 # Combined flags with path
ls ..                             # Parent directory
```

**Sample Output**:
```
Detailed:
2024-01-15 10:30    rw-r--r--    notes.txt
2024-01-14 15:45    rwxr-xr-x    documents/

Simple:
notes.txt    documents/
```

---

#### 4. `cd` - Change Directory
**Syntax**: `cd <path>`
- Navigates to specified directory
- **Requires**: Execute permission
- Special paths: `.` (current), `..` (parent), `/` (root)

**Examples**:
```bash
cd documents                      # Enter subdirectory
cd ..                            # Go to parent
cd /                             # Go to root
cd /user/docs/projects           # Absolute path
```

**Output**: `Changed directory to: documents`

---

#### 5. `pwd` - Print Working Directory
**Syntax**: `pwd`
- Shows current directory path

**Example**:
```bash
pwd
```
**Output**: `Current directory: /user/documents`

---

#### 6. `rm` - Remove File or Directory
**Syntax**: `rm [-r] [-f] <name>`
- Deletes files or directories
- `-r`: Recursive (delete non-empty directories)
- `-f`: Force (no confirmation)
- **Requires**: Write permission for the item

**Examples**:
```bash
rm oldfile.txt                    # Delete file (confirmation)
rm -f temp.txt                    # Force delete
rm -r emptydir                    # Delete empty directory
rm -rf oldproject/                # Force recursive delete
```

**Confirmation Dialog**: Appears for non-force deletions

---

#### 7. `mv` - Move or Rename
**Syntax**: `mv <source> <destination>`
- Moves files/directories or renames them
- **Requires**: Write permission in both source and destination directories

**Examples**:
```bash
mv oldname.txt newname.txt        # Rename file
mv file.txt ../                   # Move to parent
mv /source/file /dest/            # Absolute paths
mv dir/ ../parent/                # Move directory
```

**Output**: `File 'oldname.txt' moved successfully to 'newname.txt'.`

---

#### 8. `cp` - Copy
**Syntax**: `cp <source> <destination>`
- Copies files/directories
- Note: Directory copy creates a reference, not deep copy
- **Requires**: Write permission in destination

**Examples**:
```bash
cp original.txt backup/           # Copy file
cp -r sourcedir/ destdir/         # Copy directory (if implemented)
```

**Output**: `File 'original.txt' copied successfully to 'backup'.`

---

#### 9. `cat` - Display File Content
**Syntax**: `cat <file_path>`
- Shows file contents
- Follows symbolic links
- **Requires**: Read permission

**Examples**:
```bash
cat notes.txt
cat /user/docs/readme.md
cat ./config.json
```

**Output**:
```
=== Content of 'notes.txt' ===
This is the file content.
Line 2.
=== End of file (42 characters) ===
```

---

#### 10. `echo` - Write to File
**Syntax**: `echo "content" <filename> [-t "yyyy-MM-dd HH:mm"]`
- Writes content to file (creates if doesn't exist)
- `-t`: Set custom timestamp
- **Requires**: Write permission in directory

**Examples**:
```bash
echo "Hello World" greeting.txt
echo "Line 1\nLine 2" log.txt -t "2024-01-15 14:30"
```

**Output**: `Saving content to file: greeting.txt`

---

### Link Operations

#### 11. `ln` - Create Link
**Syntax**: `ln [-s] <target> <link_name>`
- Creates hard or symbolic links
- `-s`: Create symbolic link (can link directories)
- Without `-s`: Create hard link (files only)
- **Requires**: Write permission for target

**Examples**:
```bash
ln original.txt link.txt          # Hard link
ln -s /path/to/file symlink.txt   # Symbolic link
ln -s ../parent/dir linkdir       # Directory symbolic link
```

**Output**: `Soft link 'symlink.txt' created successfully pointing to '/path/to/file'.`

---

### Information & Metadata

#### 12. `stat` - File/Directory Information
**Syntax**: `stat [-l] <path>`
- Shows detailed information
- `-l`: Extended information (owner, parent, link type)

**Examples**:
```bash
stat file.txt
stat -l /user/docs
stat .
```

**Sample Output**:
```
Information for: file.txt
----------------------------------------
Name: file.txt
Type: File
Content: hello
Size: 1024 bytes
Created: 2024-01-15 10:30
Permissions: rw-r--r--
----------------------------------------
File information retrieved successfully.
```

---

#### 13. `tree` - Directory Tree
**Syntax**: `tree [-n<depth>] [path]`
- Displays directory structure as tree
- `-n<depth>`: Limit display depth (e.g., `-n3` for 3 levels)
- Shows directories and files with icons

**Examples**:
```bash
tree                            # Current directory
tree /user                      # Specific path
tree -n2                       # Limit to 2 levels
tree -n3 /user/docs
```

**Sample Output**:
```
└── root
    ├── documents
    │   ├── notes.txt
    │   └── projects
    └── downloads
```

---

### Permission Management

#### 14. `chmod` - Change Permissions
**Syntax**: `chmod <mode> <file/directory>`
- Changes permissions for file or directory
- **Modes**:
  - **Numeric**: `755`, `644`, `777` (owner/group/others in octal)
  - **Symbolic**: `u+rwx,g+rx,o+r` (user/group/others with +, -, =)
- **Requires**: Write permission for the item

**Examples**:
```bash
chmod 755 script.sh              # rwxr-xr-x
chmod 644 config.txt             # rw-r--r--
chmod u+rwx,g+rx,o+r dir/        # Add permissions
chmod g-w file.txt               # Remove write from group
chmod o=r-- other.txt            # Set exact permissions
```

**Output**: `Permissions changed successfully: rwxr-xr-x`

---

### Search Operations

#### 15. `find` - Search Files/Directories
**Syntax**: `find <path> <option> <pattern>`
- Searches recursively from specified path
- **Options**:
  - `-name`: Search by name pattern (supports wildcards: *, ?)
  - `-type`: Search by type (`f` for files, `d` for directories)

**Examples**:
```bash
find . -name "*.txt"            # Find all .txt files
find / -name "config*"          # Find starting with "config"
find . -type f                  # Find all files
find ~ -type d                  # Find all directories in home
find . -name "*.json" -type f   # Combined (if supported)
```

**Output**: Lists full paths of matching items

---

### User & Group Management

#### 16. `usertype` - Show User Type
**Syntax**: `usertype`
- Shows current user's type in loaded system file

**Example**:
```bash
usertype
```
**Output**: `Current User Type: Admin`

---

#### 17. `changeusertype` - Change User Type
**Syntax**: `changeusertype <username> <type>`
- Changes user type for specified user
- **Types**: `owner`, `group`, `other`, `admin`
- **Requires**: Admin privileges

**Examples**:
```bash
changeusertype alice group
changeusertype bob admin
```

**Output**: `Successfully changed user type for 'alice' to 'Group'.`

---

#### 18. `addgroup` - Add User to Group
**Syntax**: `addgroup <username> <permission_type>`
- Adds current user to another user's group
- **Permission Types**: `owner`, `group`, `other`, `admin`

**Examples**:
```bash
addgroup alice group            # Add as group member
addgroup bob owner              # Add as owner
```

**Output**: `Successfully added user 'alice' to the group with 'Group' permission level.`

---

#### 19. `rmgroup` - Remove from Group
**Syntax**: `rmgroup <groupname> <username>`
- Removes user from a group
- Can only remove from groups you own

**Example**:
```bash
rmgroup mygroup alice
```

**Output**: `Successfully removed user 'alice' from group 'mygroup'.`

---

### System File Management

#### 20. `load` - Load Another System File
**Syntax**: `load <systemname> [commitversion]`
- Loads another user's file system
- Default commit: "main"
- Requires permission via groups

**Examples**:
```bash
load alice                      # Load alice's main commit
load bob feature-branch         # Load specific commit
```

**Output**: `Access granted as Group member to system file 'alice'.`

---

#### 21. `systemfile` - Show System Information
**Syntax**: `systemfile`
- Shows current system file and commit

**Example**:
```bash
systemfile
```
**Output**:
```
Current System File: alice
Current Commit: main
Current Directory: /user/documents
```

---

### Version Control Commands

#### 22. `commit` - Commit Operations
**Syntax**: `commit <option> [name]`
- Manages commit versions
- **Options**:
  - `-v`: Show current commit version
  - `-l`: List all commits
  - `-m "name"`: Create new commit
  - `-d "name"`: Delete commit (cannot delete current)

**Examples**:
```bash
commit -v                        # Show version
commit -l                        # List commits
commit -m "backup_jan_15"       # Create commit
commit -d "old_backup"          # Delete commit
```

**Sample Output (list)**:
```
Commit history for system: alice
----------------------------------------
main ← Current
backup_jan_14
experiment
----------------------------------------
Total commits: 3
Current commit: main
```

---

#### 23. `revert` - Revert to Commit
**Syntax**: `revert <commit_name>`
- Loads a previous commit as current state
- Does not delete current commit

**Example**:
```bash
revert backup_jan_14
```

**Output**: `Successfully reverted to commit 'backup_jan_14'.`

---

### Tree View Operations

#### 24. `open` - Expand Tree View
**Syntax**: `open [option|path]`
- Controls tree view expansion
- **Options**:
  - `-a`: Expand all nodes
  - `-c`: Collapse to current path only
  - `<path>`: Expand specific path

**Examples**:
```bash
open -a                         # Expand all
open -c                         # Collapse to current
open /user/documents            # Expand specific path
```

**Output**: `All tree view nodes expanded.`

---

#### 25. `close` - Collapse Tree View
**Syntax**: `close`
- Collapses all tree view nodes

**Example**:
```bash
close
```
**Output**: `Tree view collapsed.`

------

#### 26. `logout` - Log Out of System
**Syntax**: `logout`
- Logs out the current user
- Saves all changes automatically
- Returns to the login page

**Example**:
```bash
logout
```
**Output**: `All changes saved. Logging out...`

---

## Advanced Usage

### Working with Multiple Users

1. **Share Your File System**:
   ```bash
   # User alice wants to share with bob
   addgroup bob group          # Alice adds Bob with group access
   ```

2. **Bob Accesses Alice's System**:
   ```bash
   # Bob logs in and loads Alice's system
   load alice
   # Bob now has group permissions in Alice's file system
   ```

3. **Alice Controls Permissions**:
   ```bash
   # Alice can change Bob's access level
   changeusertype bob owner
   # Or remove access
   rmgroup alice bob
   ```

### Version Control Workflow

1. **Initial Setup**:
   ```bash
   systemfile                  # Check current: alice, main
   ```

2. **Make Changes**:
   ```bash
   mkdir new_feature
   cd new_feature
   touch code.cs "// New feature"
   ```

3. **Create Commit**:
   ```bash
   commit -m "new_feature_v1"
   ```

4. **Experiment Safely**:
   ```bash
   # Make experimental changes
   touch experimental.txt
   # If experiment fails, revert
   revert new_feature_v1
   ```

5. **Manage History**:
   ```bash
   commit -l                    # View all commits
   commit -d "bad_experiment"  # Delete unwanted commit
   ```

### Permission Management Examples

1. **Secure a Directory**:
   ```bash
   mkdir private
   chmod 700 private           # Only owner: rwx------
   ```

2. **Share Read-Only**:
   ```bash
   mkdir shared_read
   chmod 755 shared_read       # Owner: rwx, Others: r-x
   ```

3. **Collaborative Directory**:
   ```bash
   mkdir collaborative
   chmod 775 collaborative     # Owner/Group: rwx, Others: r-x
   ```

4. **Public File**:
   ```bash
   touch public.txt
   chmod 644 public.txt        # Owner: rw-, Others: r--
   ```

### Working with Links

1. **Create Shortcut to Frequently Used Directory**:
   ```bash
   ln -s /user/docs/projects/long_path/current/source src
   ```

2. **Backup Reference**:
   ```bash
   cp important.txt backup/
   ln important.txt backup/latest_important.txt
   ```

3. **Directory Alias**:
   ```bash
   ln -s /user/documents/docs
   # Now 'cd docs' works from anywhere
   ```

## Error Handling & Troubleshooting

### Common Error Messages

| Error Message | Cause | Solution |
|--------------|-------|----------|
| `Permission denied` | Insufficient permissions | Check user type and permissions with `stat` |
| `Path not found` | Invalid or non-existent path | Verify path with `pwd` and `ls` |
| `File already exists` | Duplicate filename | Choose different name or remove existing |
| `Directory not empty` | Trying to delete non-empty dir | Use `rm -r` for recursive delete |
| `Invalid command syntax` | Wrong command format | Check command reference above |
| `User does not exist` | Invalid username | Verify username with system administrator |
| `Commit not found` | Invalid commit name | Use `commit -l` to list available commits |

### Debug Tips

1. **Check Current State**:
   ```bash
   pwd                          # Where am I?
   systemfile                   # Which system/commit?
   usertype                     # What are my permissions?
   ```

2. **Verify Permissions**:
   ```bash
   stat .                       # Check current directory
   ls -l                        # See all items with permissions
   ```

3. **Test Access**:
   ```bash
   touch test.txt              # Can I write here?
   cat test.txt                # Can I read here?
   cd subdir                   # Can I enter here?
   ```

4. **Review Command History**:
   - Scroll through history with up/down arrows
   - All outputs are logged in command history panel

## Project Structure

### Data Persistence

1. **Users**: Stored in `users.json` with password hashes
2. **File Systems**: Each user+commit combination has separate JSON file
3. **Automatic Saving**: Most operations auto-save to current commit
4. **Manual Save**: Use implicit save via operations

## Limitations & Known Issues

### Current Limitations
1. **No Recursive Copy**: `cp` for directories creates references only
2. **Basic Search**: `find` supports only name and type patterns
3. **No File Editing**: Cannot edit file content directly, must recreate
4. **Memory Based**: Entire file system loads into memory
5. **No Network Support**: Single-user, local application only
6. **Limited Wildcards**: Only `*` and `?` in find command

### Performance Considerations
- Large file systems may impact performance
- Each commit creates full copy (space intensive)
- No compression of stored data

### Security Notes
- Passwords are hashed using SHA256
- No encryption for file system data
- Permission system is basic (no ACLs)
- Admin users have full access to all systems

## Future Enhancements

### Planned Features
1. **File Editing**: In-place content modification
2. **Recursive Operations**: Full directory copy/move
3. **Advanced Search**: Regular expression support
4. **Compression**: Gzip for commit storage
5. **Network Mode**: Multi-user collaboration
6. **File Upload/Download**: Integration with real file system
7. **Permission Inheritance**: Unix-like permission inheritance
8. **Disk Usage Statistics**: Visual representation of space used

### Community Contributions
Areas where contributors can help:
- Implement recursive operations
- Add file editing capabilities
- Improve search functionality
- Create better UI/UX
- Add unit tests
- Performance optimization

## Contributing

We welcome contributions! Here's how to help:

### Reporting Issues
1. Check existing issues to avoid duplicates
2. Use the issue template with:
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots if applicable
   - Environment details

### Submitting Code
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests if applicable
5. Update documentation
6. Submit pull request

### Coding Standards
- Follow C# naming conventions
- Add XML documentation comments
- Use try-catch with meaningful error messages
- Update README for new features

## License

MIT License

---

**Happy File System Simulating!** 🗂️