def match_with_wildcard(pattern, string):
    
    # Wenn das Muster leer ist, überprüfe, ob auch der String leer ist
    if not pattern:
        return not string

    if (len(string) == 1 and string == "*"):
        return True
    
    # Wenn das Muster und der String ein Zeichen lang sind
    if len(pattern) == 1 and len(string) == 1:
        return pattern == string or pattern == "*"

    # Wenn das Muster ein Zeichen und ein Platzhalter enthält
    if len(pattern) == 2 and pattern[1] == "*":
        return pattern[0] == string or pattern[0] == "*"

    # Wenn das Muster und der String gleich sind
    if pattern == string:
        return True

    # Wenn der String leer ist, überprüfe, ob das Muster nur aus Platzhaltern besteht
    if not string:
        return all(x == "*" for x in pattern)

    # Wenn das erste Zeichen des Musters und des Strings übereinstimmt
    if pattern[0] == string[0]:
        # Rekursiv den Rest des Musters und des Strings überprüfen
        return match_with_wildcard(pattern[1:], string[1:])
    
    if pattern[0] == "*":
        return match_with_wildcard(pattern, string[1:]) or match_with_wildcard(pattern[1:], string)

    return False

# Beispielaufrufe
pattern = "abc*def"
strings = ["abcdef", "abcXYZde*", "def"]
for s in strings:
    print(match_with_wildcard(pattern, s))
