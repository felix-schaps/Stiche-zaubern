import csv
import xml.etree.ElementTree as ET

blank_spalten_zwischen_dateien = 2
output = 'beispiel.csv'

xml_files = ['test2.xml', 'test.xml']

def create_tst13(xml_files, csv_file, filter_structure=None):
    # Öffne die XML-Datei und parse sie
    number_files = len(xml_files)
    trees = [ET.parse(xml_file) for xml_file in xml_files]
    roots = [tree.getroot() for tree in trees]
    
    # Öffne die CSV-Datei für das Schreiben
    with open(csv_file, 'w', newline='') as csvfile:
        # Setze den Trenner auf Semikolon
        csvwriter = csv.writer(csvfile, delimiter=';')
        
        # Funktion zum Überprüfen, ob die Hierarchie mit dem Filter übereinstimmt
        def matches_filter(hierarchy):
            if filter_structure is None:
                return 1
            for filter_item in filter_structure:
                if filter_item.startswith(hierarchy):
                    return 1
                if hierarchy.startswith(filter_item):
                    return 2
            return 0
        
        # Rekursive Funktion zum Durchlaufen der XML-Struktur und Schreiben in die CSV-Datei
        def write_to_csv(matrix, parent_hierarchy=''):
            print("Aufruf mit")
            print(parent_hierarchy)
            check_is_list_tree = next((tree for tree in matrix if tree is not None and len(tree)> 1), None)
            if check_is_list_tree is not None and all(child.tag == check_is_list_tree[0].tag for child in check_is_list_tree):
                k = 1
                while True:
                    if all(len(tree)<k for tree in matrix):
                        csvwriter.writerow('')
                        return
                    current_hierarchy = f"{parent_hierarchy}[{k}]"
                    child_matrix = [matrix[i][k-1] if k <= len(matrix[i]) else None for i in range(number_files)]
                    if matches_filter(parent_hierarchy+'[') != 1:
                        write_to_csv(child_matrix,parent_hierarchy)
                        csvwriter.writerow('')
                    elif matches_filter(current_hierarchy) != 0:
                        write_to_csv(child_matrix,current_hierarchy)
                    k = k+1
            
            #Sammle alle vorkommenden Kindernamen
            for tree in matrix:
                if tree is None:
                    continue
                tags = []
                for child in tree:
                    if child.tag not in tags:
                        tags.append(child.tag)
            
            #Filtere nur nach auszugebenden Kindern
            relevant_tags = []
            for tag in tags:
                current_hierarchy = f"{parent_hierarchy}/{tag}" if parent_hierarchy else tag
                if matches_filter(current_hierarchy) != 0:
                    relevant_tags.append(tag)
            
            for tag in relevant_tags:
                current_hierarchy = f"{parent_hierarchy}/{tag}" if parent_hierarchy else tag
                child_matrix = []
                for i in range(number_files):
                    if matrix[i] is None:
                        child_matrix.append(None)
                        continue
                    child_matrix.append(next((child for child in matrix[i] if child is not None and child.tag == tag),None))
                        
                if any(child is not None and len(child) > 0 for child in child_matrix):# Wenn das Kind Unterlemente hat, rufe die Funktion rekursiv auf
                    write_to_csv(child_matrix, current_hierarchy)
                    if matches_filter(current_hierarchy) == 1:
                        csvwriter.writerow('')
                else:
                    row = ['']*(len(matrix)*(blank_spalten_zwischen_dateien+1)+2)
                    row[0] = parent_hierarchy
                    row[1] = tag
                    j = 2
                    for i in range(number_files):
                        child = child_matrix[i]
                        row[j] = child.text if child is not None and child.text else ''
                        j = j + blank_spalten_zwischen_dateien + 1
                    
                    csvwriter.writerow(row)
        
        # Starte die Rekursion mit dem Wurzelelement
        write_to_csv(roots)

# Beispielaufruf der Funktion mit den Dateinamen
filter_structure = ['vertragsbausteine']
create_tst13(xml_files, output, filter_structure)
