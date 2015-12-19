# Comparison Method #

  1. Compare files in current source directory
    1. New conflict if file is not in target
    1. Compare file contents byte-for-byte, new conflict if not equal
  1. New conflict for each file in target which isn't in source
  1. Compare sub-directories in current source directory
    1. New conflict if directory not in target
    1. Go to 1 with sub-directory as current directory
  1. New conflict for each sub-directory in target which isn't in source

# Resolutions #

| **Conflict** | **Possible Resolutions`*`** |
|:-------------|:----------------------------|
| Directory not in source | Copy to source / Delete     |
| Directory not in target | Copy to target              |
| File modified in target | Copy to target / Copy to source |
| File not in source | Copy to source / Delete     |
| File not in target | Copy to target              |

`*` All conflicts may be ignored